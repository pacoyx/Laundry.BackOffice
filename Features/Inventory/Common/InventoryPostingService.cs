using Microsoft.EntityFrameworkCore;

/// <summary>Error de negocio de inventario (stock insuficiente, datos inválidos). Se devuelve como 400.</summary>
public class InventoryException : Exception
{
    public InventoryException(string message) : base(message) { }
}

/// <summary>Línea a registrar. Quantity siempre positiva; el tipo de movimiento define si entra o sale.</summary>
public record InventoryDraftLine(int ProductId, decimal Quantity, decimal? UnitCost);

public record InventoryMovementDraft
{
    public InventoryMovementType Type { get; init; }
    public DateTime MovementDate { get; init; }
    public int WarehouseId { get; init; }
    public int? TargetWarehouseId { get; init; }
    public int? AreaId { get; init; }
    public string? Reason { get; init; }
    public int? PurchaseInvoiceId { get; init; }
    public long? SaleId { get; init; }
    public int? CreatedByUserId { get; init; }
    public List<InventoryDraftLine> Lines { get; init; } = new();
}

public interface IInventoryPostingService
{
    /// <summary>Registra un movimiento y actualiza los saldos. Usa la transacción activa si existe.</summary>
    Task<InventoryMovement> PostAsync(InventoryMovementDraft draft, CancellationToken cancellationToken = default);

    /// <summary>Anula un movimiento con un movimiento inverso (Reversal). Nunca borra.</summary>
    Task<InventoryMovement> ReverseAsync(long movementId, string reason, int? userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Único punto que modifica existencias. Costo promedio ponderado, stock negativo bloqueado, saldo
/// guardado en cada línea (kardex) y bloqueo pesimista del saldo para evitar carreras.
/// </summary>
public class InventoryPostingService : IInventoryPostingService
{
    private const int CostDecimals = 4;

    private static readonly InventoryMovementType[] EntryTypes =
        { InventoryMovementType.InitialBalance, InventoryMovementType.PurchaseIn, InventoryMovementType.AdjustmentIn };

    private static readonly InventoryMovementType[] ExitTypes =
        { InventoryMovementType.AdjustmentOut, InventoryMovementType.Consumption, InventoryMovementType.SaleOut };

    private readonly RecepcionDbContext _context;

    public InventoryPostingService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryMovement> PostAsync(InventoryMovementDraft draft, CancellationToken cancellationToken = default)
    {
        await ValidateDraftAsync(draft, cancellationToken);

        return await InTransactionAsync(async () =>
        {
            var movement = NewMovement(draft.Type, draft.MovementDate, draft.WarehouseId, draft.CreatedByUserId);
            movement.TargetWarehouseId = draft.TargetWarehouseId;
            movement.AreaId = draft.AreaId;
            movement.Reason = draft.Reason?.Trim();
            movement.PurchaseInvoiceId = draft.PurchaseInvoiceId;
            movement.SaleId = draft.SaleId;

            var balances = new Dictionary<(int, int), ProductStock>();
            var productNames = await ProductNamesAsync(draft.Lines.Select(l => l.ProductId), cancellationToken);

            foreach (var line in draft.Lines)
            {
                if (EntryTypes.Contains(draft.Type))
                {
                    var stock = await LockStockAsync(line.ProductId, draft.WarehouseId, balances, cancellationToken);
                    var cost = line.UnitCost ?? stock.AverageCost;
                    movement.Lines.Add(ApplyEntry(stock, line.ProductId, draft.WarehouseId, line.Quantity, cost));
                }
                else if (ExitTypes.Contains(draft.Type))
                {
                    var stock = await LockStockAsync(line.ProductId, draft.WarehouseId, balances, cancellationToken);
                    movement.Lines.Add(ApplyExit(stock, line.ProductId, draft.WarehouseId, line.Quantity, null, productNames));
                }
                else if (draft.Type == InventoryMovementType.Transfer)
                {
                    var source = await LockStockAsync(line.ProductId, draft.WarehouseId, balances, cancellationToken);
                    var outLine = ApplyExit(source, line.ProductId, draft.WarehouseId, line.Quantity, null, productNames);
                    movement.Lines.Add(outLine);

                    var target = await LockStockAsync(line.ProductId, draft.TargetWarehouseId!.Value, balances, cancellationToken);
                    movement.Lines.Add(ApplyEntry(target, line.ProductId, draft.TargetWarehouseId.Value, line.Quantity, outLine.UnitCost));
                }
                else
                {
                    throw new InventoryException("Tipo de movimiento no permitido.");
                }
            }

            await SaveWithNumberAsync(movement, cancellationToken);
            return movement;
        }, cancellationToken);
    }

    public async Task<InventoryMovement> ReverseAsync(long movementId, string reason, int? userId, CancellationToken cancellationToken = default)
    {
        return await InTransactionAsync(async () =>
        {
            var original = await _context.InventoryMovements
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == movementId, cancellationToken)
                ?? throw new InventoryException("Movimiento no encontrado.");

            if (original.Type == InventoryMovementType.Reversal)
                throw new InventoryException("Un movimiento de anulación no se puede anular.");
            if (original.Status == InventoryMovementStatus.Cancelled)
                throw new InventoryException("El movimiento ya está anulado.");

            var reversal = NewMovement(InventoryMovementType.Reversal, NowLima(), original.WarehouseId, userId);
            reversal.TargetWarehouseId = original.TargetWarehouseId;
            reversal.AreaId = original.AreaId;
            reversal.PurchaseInvoiceId = original.PurchaseInvoiceId;
            reversal.SaleId = original.SaleId;
            reversal.ReversedMovementId = original.Id;
            reversal.Reason = string.IsNullOrWhiteSpace(reason) ? $"Anulación de {original.Number}" : reason.Trim();

            var balances = new Dictionary<(int, int), ProductStock>();
            var productNames = await ProductNamesAsync(original.Lines.Select(l => l.ProductId), cancellationToken);

            // Se revierte en orden inverso: en una transferencia primero sale del destino y luego vuelve al origen.
            foreach (var line in original.Lines.OrderByDescending(l => l.Id))
            {
                var stock = await LockStockAsync(line.ProductId, line.WarehouseId, balances, cancellationToken);
                reversal.Lines.Add(line.Quantity > 0
                    // Era un ingreso: se retira la misma cantidad al mismo costo con que entró.
                    ? ApplyExit(stock, line.ProductId, line.WarehouseId, line.Quantity, line.UnitCost, productNames)
                    // Era una salida: se devuelve la cantidad al costo con que salió.
                    : ApplyEntry(stock, line.ProductId, line.WarehouseId, -line.Quantity, line.UnitCost));
            }

            original.Status = InventoryMovementStatus.Cancelled;
            await SaveWithNumberAsync(reversal, cancellationToken);
            original.ReversalMovementId = reversal.Id;
            await _context.SaveChangesAsync(cancellationToken);
            return reversal;
        }, cancellationToken);
    }

    // ---- reglas de costo ----

    private static InventoryMovementLine ApplyEntry(ProductStock stock, int productId, int warehouseId, decimal quantity, decimal unitCost)
    {
        var newQuantity = stock.Quantity + quantity;
        stock.AverageCost = newQuantity == 0
            ? stock.AverageCost
            : Math.Round((stock.Quantity * stock.AverageCost + quantity * unitCost) / newQuantity, CostDecimals);
        stock.Quantity = newQuantity;
        stock.LastMovementAt = DateTime.UtcNow;

        return Line(productId, warehouseId, quantity, unitCost, stock);
    }

    /// <param name="removalCost">
    /// null = salida normal al costo promedio (no cambia el promedio).
    /// Con valor = se retira un ingreso anulado a su costo original (recalcula el promedio).
    /// </param>
    private static InventoryMovementLine ApplyExit(ProductStock stock, int productId, int warehouseId, decimal quantity,
        decimal? removalCost, IReadOnlyDictionary<int, string> productNames)
    {
        if (stock.Quantity - quantity < 0)
        {
            var name = productNames.TryGetValue(productId, out var n) ? n : $"#{productId}";
            throw new InventoryException(
                $"Stock insuficiente de \"{name}\": disponible {stock.Quantity:0.####}, se requiere {quantity:0.####}.");
        }

        var unitCost = removalCost ?? stock.AverageCost;
        var newQuantity = stock.Quantity - quantity;

        if (removalCost.HasValue && newQuantity > 0)
        {
            var recalculated = Math.Round((stock.Quantity * stock.AverageCost - quantity * removalCost.Value) / newQuantity, CostDecimals);
            stock.AverageCost = recalculated >= 0 ? recalculated : stock.AverageCost;
        }

        stock.Quantity = newQuantity;
        stock.LastMovementAt = DateTime.UtcNow;

        return Line(productId, warehouseId, -quantity, unitCost, stock);
    }

    private static InventoryMovementLine Line(int productId, int warehouseId, decimal signedQuantity, decimal unitCost, ProductStock stock) => new()
    {
        ProductId = productId,
        WarehouseId = warehouseId,
        Quantity = signedQuantity,
        UnitCost = Math.Round(unitCost, CostDecimals),
        TotalCost = Math.Round(signedQuantity * unitCost, CostDecimals),
        BalanceQuantity = stock.Quantity,
        BalanceAverageCost = stock.AverageCost
    };

    // ---- infraestructura ----

    /// <summary>
    /// Lee el saldo con UPDLOCK/HOLDLOCK: otro movimiento sobre el mismo producto y almacén espera a que
    /// termine esta transacción (también evita que dos transacciones creen el mismo saldo a la vez).
    /// </summary>
    private async Task<ProductStock> LockStockAsync(int productId, int warehouseId,
        Dictionary<(int, int), ProductStock> cache, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue((productId, warehouseId), out var cached))
        {
            return cached;
        }

        var stock = await _context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = {productId} AND WarehouseId = {warehouseId}")
            .SingleOrDefaultAsync(cancellationToken); // PK (ProductId, WarehouseId): a lo sumo una fila; evita el warning EF 10103

        if (stock is null)
        {
            stock = new ProductStock { ProductId = productId, WarehouseId = warehouseId, Quantity = 0, AverageCost = 0 };
            _context.ProductStocks.Add(stock);
        }

        cache[(productId, warehouseId)] = stock;
        return stock;
    }

    private async Task SaveWithNumberAsync(InventoryMovement movement, CancellationToken cancellationToken)
    {
        _context.InventoryMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);
        movement.Number = $"MOV-{movement.Id:D8}";
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        // Si el llamador ya abrió una transacción (p. ej. al registrar una compra), se participa de ella.
        if (_context.Database.CurrentTransaction is not null)
        {
            return await work();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var result = await work();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task ValidateDraftAsync(InventoryMovementDraft draft, CancellationToken cancellationToken)
    {
        if (draft.Lines.Count == 0)
            throw new InventoryException("El movimiento debe tener al menos una línea.");
        if (draft.Lines.Any(l => l.Quantity <= 0))
            throw new InventoryException("Las cantidades deben ser mayores a cero.");
        if (draft.Lines.Any(l => l.UnitCost is < 0))
            throw new InventoryException("El costo unitario no puede ser negativo.");

        var warehouseIds = new List<int> { draft.WarehouseId };
        if (draft.TargetWarehouseId.HasValue) warehouseIds.Add(draft.TargetWarehouseId.Value);
        var activeWarehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id) && w.IsActive)
            .Select(w => w.Id)
            .ToListAsync(cancellationToken);
        if (!activeWarehouses.Contains(draft.WarehouseId))
            throw new InventoryException("El almacén no existe o está inactivo.");

        if (draft.Type == InventoryMovementType.Transfer)
        {
            if (draft.TargetWarehouseId is null || draft.TargetWarehouseId == draft.WarehouseId)
                throw new InventoryException("La transferencia requiere un almacén destino distinto al de origen.");
            if (!activeWarehouses.Contains(draft.TargetWarehouseId.Value))
                throw new InventoryException("El almacén destino no existe o está inactivo.");
        }

        if (draft.Type == InventoryMovementType.Consumption)
        {
            if (draft.AreaId is null || !await _context.Areas.AnyAsync(a => a.Id == draft.AreaId, cancellationToken))
                throw new InventoryException("El consumo interno requiere un área válida.");
        }

        if (draft.Type is InventoryMovementType.InitialBalance or InventoryMovementType.PurchaseIn
            && draft.Lines.Any(l => l.UnitCost is null))
            throw new InventoryException("Los ingresos de inventario inicial y por compra requieren costo unitario.");

        var productIds = draft.Lines.Select(l => l.ProductId).Distinct().ToList();
        var trackedProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.TracksStock)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
        var invalid = productIds.Except(trackedProducts).ToList();
        if (invalid.Count > 0)
            throw new InventoryException($"Hay productos inexistentes o que no controlan stock (ids: {string.Join(", ", invalid)}).");
    }

    private async Task<Dictionary<int, string>> ProductNamesAsync(IEnumerable<int> productIds, CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToList();
        return await _context.Products
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);
    }

    private static InventoryMovement NewMovement(InventoryMovementType type, DateTime date, int warehouseId, int? userId) => new()
    {
        Type = type,
        Status = InventoryMovementStatus.Posted,
        MovementDate = date,
        WarehouseId = warehouseId,
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = userId,
        Number = "MOV-PEND"
    };

    public static DateTime NowLima() =>
        TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
}

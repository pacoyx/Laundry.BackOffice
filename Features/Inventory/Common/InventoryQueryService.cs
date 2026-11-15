using Microsoft.EntityFrameworkCore;

public interface IInventoryQueryService
{
    Task<List<StockItemDto>> GetStockAsync(int? warehouseId, int? categoryId, bool belowMinOnly, string? search, CancellationToken cancellationToken = default);
    Task<List<ProductStockByWarehouseDto>> GetProductStockAsync(int productId, CancellationToken cancellationToken = default);
    Task<KardexDto?> GetKardexAsync(int productId, int warehouseId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<InventoryMovementDetailDto?> GetMovementAsync(long id, CancellationToken cancellationToken = default);
}

public class InventoryQueryService : IInventoryQueryService
{
    private readonly RecepcionDbContext _context;

    public InventoryQueryService(RecepcionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Con almacén: todos los productos que controlan stock con su saldo en ese almacén (0 si no tiene).
    /// Sin almacén: saldo total por producto sumando los almacenes activos.
    /// </summary>
    public async Task<List<StockItemDto>> GetStockAsync(int? warehouseId, int? categoryId, bool belowMinOnly, string? search, CancellationToken cancellationToken = default)
    {
        var products = _context.Products.AsNoTracking().Where(p => p.TracksStock && p.Status == "A");
        if (categoryId.HasValue) products = products.Where(p => p.CategoryProdId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            products = products.Where(p => p.Name.Contains(term) || (p.Code != null && p.Code.Contains(term)));
        }

        var productList = await products
            .Select(p => new
            {
                p.Id, p.Code, p.Name, p.MinStock,
                CategoryName = p.CategoryProduct != null ? p.CategoryProduct.Name : null,
                UnitName = p.UnitMeasurement != null ? p.UnitMeasurement.Name : null
            })
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var productIds = productList.Select(p => p.Id).ToList();
        var stockQuery = _context.ProductStocks.AsNoTracking()
            .Where(s => productIds.Contains(s.ProductId) && s.Warehouse!.IsActive);
        if (warehouseId.HasValue) stockQuery = stockQuery.Where(s => s.WarehouseId == warehouseId.Value);
        var stocks = await stockQuery
            .Select(s => new { s.ProductId, s.Quantity, s.AverageCost })
            .ToListAsync(cancellationToken);

        string? warehouseName = null;
        if (warehouseId.HasValue)
        {
            warehouseName = await _context.Warehouses.Where(w => w.Id == warehouseId.Value).Select(w => w.Name).FirstOrDefaultAsync(cancellationToken);
        }

        var result = productList.Select(p =>
        {
            var rows = stocks.Where(s => s.ProductId == p.Id).ToList();
            var quantity = rows.Sum(r => r.Quantity);
            var value = rows.Sum(r => r.Quantity * r.AverageCost);
            return new StockItemDto
            {
                ProductId = p.Id,
                ProductCode = p.Code,
                ProductName = p.Name,
                CategoryName = p.CategoryName,
                UnitName = p.UnitName,
                WarehouseId = warehouseId,
                WarehouseName = warehouseName,
                Quantity = quantity,
                AverageCost = quantity != 0 ? Math.Round(value / quantity, 4) : (rows.FirstOrDefault()?.AverageCost ?? 0),
                TotalValue = Math.Round(value, 2),
                MinStock = p.MinStock,
                BelowMin = p.MinStock > 0 && quantity < p.MinStock
            };
        });

        if (belowMinOnly) result = result.Where(r => r.BelowMin);
        return result.ToList();
    }

    public async Task<List<ProductStockByWarehouseDto>> GetProductStockAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductStocks.AsNoTracking()
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.Warehouse!.Name)
            .Select(s => new ProductStockByWarehouseDto
            {
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse!.Name,
                BranchName = s.Warehouse.BranchSales != null ? s.Warehouse.BranchSales.Description : string.Empty,
                Quantity = s.Quantity,
                AverageCost = s.AverageCost,
                TotalValue = Math.Round(s.Quantity * s.AverageCost, 2)
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Kardex de un producto en un almacén. Las líneas se ordenan por orden de registro (Id), que es el
    /// orden en que se calcularon los saldos guardados en cada línea.
    /// </summary>
    public async Task<KardexDto?> GetKardexAsync(int productId, int warehouseId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.Code, p.Name, UnitName = p.UnitMeasurement != null ? p.UnitMeasurement.Name : null })
            .FirstOrDefaultAsync(cancellationToken);
        var warehouse = await _context.Warehouses.AsNoTracking()
            .Where(w => w.Id == warehouseId)
            .Select(w => new { w.Id, w.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null || warehouse is null) return null;

        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var baseQuery = _context.InventoryMovementLines.AsNoTracking()
            .Where(l => l.ProductId == productId && l.WarehouseId == warehouseId);

        var opening = await baseQuery
            .Where(l => l.InventoryMovement!.MovementDate < fromDate)
            .OrderByDescending(l => l.Id)
            .Select(l => new { l.BalanceQuantity, l.BalanceAverageCost })
            .FirstOrDefaultAsync(cancellationToken);

        var rows = await baseQuery
            .Where(l => l.InventoryMovement!.MovementDate >= fromDate && l.InventoryMovement.MovementDate < toDate)
            .OrderBy(l => l.Id)
            .Select(l => new
            {
                l.InventoryMovementId,
                l.InventoryMovement!.Number,
                l.InventoryMovement.MovementDate,
                l.InventoryMovement.Type,
                l.InventoryMovement.Status,
                l.InventoryMovement.Reason,
                AreaName = l.InventoryMovement.Area != null ? l.InventoryMovement.Area.Name : null,
                SourceName = l.InventoryMovement.Warehouse != null ? l.InventoryMovement.Warehouse.Name : null,
                TargetName = l.InventoryMovement.TargetWarehouse != null ? l.InventoryMovement.TargetWarehouse.Name : null,
                InvoiceSerie = l.InventoryMovement.PurchaseInvoice != null ? l.InventoryMovement.PurchaseInvoice.InvoiceSerie : null,
                InvoiceNumber = l.InventoryMovement.PurchaseInvoice != null ? l.InventoryMovement.PurchaseInvoice.InvoiceNumber : null,
                l.Quantity, l.UnitCost, l.TotalCost, l.BalanceQuantity, l.BalanceAverageCost
            })
            .ToListAsync(cancellationToken);

        var lines = rows.Select(r => new KardexLineDto
        {
            MovementId = r.InventoryMovementId,
            MovementNumber = r.Number,
            MovementDate = r.MovementDate,
            Type = r.Type,
            Status = r.Status,
            Detail = BuildDetail(r.Type, r.Reason, r.AreaName, r.SourceName, r.TargetName, r.InvoiceSerie, r.InvoiceNumber, r.Quantity),
            QuantityIn = r.Quantity > 0 ? r.Quantity : 0,
            QuantityOut = r.Quantity < 0 ? -r.Quantity : 0,
            UnitCost = r.UnitCost,
            TotalCost = Math.Abs(r.TotalCost),
            BalanceQuantity = r.BalanceQuantity,
            BalanceAverageCost = r.BalanceAverageCost,
            BalanceValue = Math.Round(r.BalanceQuantity * r.BalanceAverageCost, 2)
        }).ToList();

        var openingQty = opening?.BalanceQuantity ?? 0;
        var openingAvg = opening?.BalanceAverageCost ?? 0;
        var last = lines.LastOrDefault();

        return new KardexDto
        {
            ProductId = product.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            UnitName = product.UnitName,
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name,
            From = from,
            To = to,
            OpeningQuantity = openingQty,
            OpeningAverageCost = openingAvg,
            OpeningValue = Math.Round(openingQty * openingAvg, 2),
            TotalIn = lines.Sum(l => l.QuantityIn),
            TotalOut = lines.Sum(l => l.QuantityOut),
            ClosingQuantity = last?.BalanceQuantity ?? openingQty,
            ClosingAverageCost = last?.BalanceAverageCost ?? openingAvg,
            ClosingValue = last?.BalanceValue ?? Math.Round(openingQty * openingAvg, 2),
            Lines = lines
        };
    }

    public async Task<InventoryMovementDetailDto?> GetMovementAsync(long id, CancellationToken cancellationToken = default)
    {
        var m = await _context.InventoryMovements.AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.TargetWarehouse)
            .Include(x => x.Area)
            .Include(x => x.PurchaseInvoice)
            .Include(x => x.Lines).ThenInclude(l => l.Product!).ThenInclude(p => p.UnitMeasurement)
            .Include(x => x.Lines).ThenInclude(l => l.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (m is null) return null;

        var related = await _context.InventoryMovements.AsNoTracking()
            .Where(x => x.Id == m.ReversedMovementId || x.Id == m.ReversalMovementId)
            .ToDictionaryAsync(x => x.Id, x => x.Number, cancellationToken);
        var userName = m.CreatedByUserId.HasValue
            ? await _context.Users.Where(u => u.Id == m.CreatedByUserId.Value).Select(u => u.UserName).FirstOrDefaultAsync(cancellationToken)
            : null;

        return new InventoryMovementDetailDto
        {
            Id = m.Id,
            Number = m.Number,
            Type = m.Type,
            Status = m.Status,
            MovementDate = m.MovementDate,
            WarehouseId = m.WarehouseId,
            WarehouseName = m.Warehouse?.Name ?? string.Empty,
            TargetWarehouseId = m.TargetWarehouseId,
            TargetWarehouseName = m.TargetWarehouse?.Name,
            AreaId = m.AreaId,
            AreaName = m.Area?.Name,
            Reason = m.Reason,
            PurchaseInvoiceId = m.PurchaseInvoiceId,
            PurchaseInvoiceRef = m.PurchaseInvoice is null ? null : $"{m.PurchaseInvoice.InvoiceSerie}-{m.PurchaseInvoice.InvoiceNumber}",
            LineCount = m.Lines.Count,
            TotalCost = Math.Round(m.Lines.Where(l => l.WarehouseId == m.WarehouseId).Sum(l => Math.Abs(l.TotalCost)), 2),
            ReversedMovementId = m.ReversedMovementId,
            ReversedMovementNumber = m.ReversedMovementId.HasValue && related.TryGetValue(m.ReversedMovementId.Value, out var rn) ? rn : null,
            ReversalMovementId = m.ReversalMovementId,
            ReversalMovementNumber = m.ReversalMovementId.HasValue && related.TryGetValue(m.ReversalMovementId.Value, out var rv) ? rv : null,
            CreatedAt = m.CreatedAt,
            CreatedByUserName = userName,
            Lines = m.Lines.OrderBy(l => l.Id).Select(l => new InventoryMovementLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductCode = l.Product?.Code,
                ProductName = l.Product?.Name ?? string.Empty,
                UnitName = l.Product?.UnitMeasurement?.Name,
                WarehouseId = l.WarehouseId,
                WarehouseName = l.Warehouse?.Name ?? string.Empty,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                TotalCost = l.TotalCost,
                BalanceQuantity = l.BalanceQuantity,
                BalanceAverageCost = l.BalanceAverageCost
            }).ToList()
        };
    }

    private static string BuildDetail(InventoryMovementType type, string? reason, string? area, string? source, string? target,
        string? serie, string? number, decimal quantity)
    {
        var baseText = type switch
        {
            InventoryMovementType.PurchaseIn => $"Compra {serie}-{number}",
            InventoryMovementType.Consumption => $"Consumo{(area is null ? "" : $" - {area}")}",
            InventoryMovementType.Transfer => quantity < 0 ? $"Transferencia a {target}" : $"Transferencia desde {source}",
            _ => InventoryLabels.MovementType(type)
        };
        // En una compra el motivo es el mismo número de factura que ya muestra el texto base.
        return string.IsNullOrWhiteSpace(reason) || type == InventoryMovementType.PurchaseIn
            ? baseText
            : $"{baseText} · {reason}";
    }
}

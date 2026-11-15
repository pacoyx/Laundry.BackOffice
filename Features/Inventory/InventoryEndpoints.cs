using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class InventoryEndpoints
{
    /// <summary>Tipos que se registran a mano (los demás los generan otros procesos).</summary>
    private static readonly InventoryMovementType[] ManualTypes =
    {
        InventoryMovementType.InitialBalance, InventoryMovementType.AdjustmentIn, InventoryMovementType.AdjustmentOut,
        InventoryMovementType.Consumption, InventoryMovementType.Transfer
    };

    public static void MapInventory(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory").RequirePermission(PermissionCodes.Inventory);

        MapWarehouses(group);
        MapMovements(group);
        MapStock(group);
        MapLookups(group);
        group.MapInventoryReports();
    }

    // ---------------- Catálogos para los selectores del módulo ----------------
    // Se exponen aquí para que un usuario con permiso solo de Inventarios no dependa de los permisos
    // de RRHH (áreas), Configuración (sucursales) o Compras (productos).

    private static void MapLookups(RouteGroupBuilder group)
    {
        group.MapGet("/lookups/areas", async (RecepcionDbContext db, CancellationToken ct) =>
            Ok(await db.Areas.AsNoTracking().Where(a => a.IsActive).OrderBy(a => a.Name)
                .Select(a => new { a.Id, a.Name, a.IsActive }).ToListAsync(ct), "Áreas obtenidas correctamente"));

        group.MapGet("/lookups/branches", async (RecepcionDbContext db, CancellationToken ct) =>
            Ok(await db.BranchSales.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Description)
                .Select(b => new { b.Id, b.Description }).ToListAsync(ct), "Sucursales obtenidas correctamente"));

        group.MapGet("/lookups/products", async (string? q, int? warehouseId, RecepcionDbContext db, CancellationToken ct) =>
        {
            var term = (q ?? string.Empty).Trim();
            var query = db.Products.AsNoTracking().Where(p => p.Status == "A" && p.TracksStock);
            if (term.Length > 0)
            {
                query = query.Where(p => p.Name.Contains(term) || (p.Code != null && p.Code.Contains(term)));
            }

            var data = await query
                .OrderBy(p => p.Name)
                .Take(30)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.TracksStock,
                    UnitMeasurementDescription = p.UnitMeasurement != null ? p.UnitMeasurement.Abbreviation : string.Empty,
                    // Con almacén: saldo en ese almacén; sin almacén: total en almacenes activos.
                    Stock = db.ProductStocks
                        .Where(s => s.ProductId == p.Id && s.Warehouse!.IsActive && (warehouseId == null || s.WarehouseId == warehouseId))
                        .Sum(s => (decimal?)s.Quantity) ?? 0
                })
                .ToListAsync(ct);
            return Ok(data, "Productos obtenidos correctamente");
        });
    }

    // ---------------- Almacenes ----------------

    private static void MapWarehouses(RouteGroupBuilder group)
    {
        group.MapGet("/warehouses", async (bool? onlyActive, RecepcionDbContext db, CancellationToken ct) =>
        {
            var query = db.Warehouses.AsNoTracking();
            if (onlyActive == true) query = query.Where(w => w.IsActive);
            var data = await query
                .OrderBy(w => w.BranchSales!.Description).ThenByDescending(w => w.IsDefault).ThenBy(w => w.Name)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    BranchSalesId = w.BranchSalesId,
                    BranchName = w.BranchSales != null ? w.BranchSales.Description : string.Empty,
                    Code = w.Code,
                    Name = w.Name,
                    IsDefault = w.IsDefault,
                    IsActive = w.IsActive
                })
                .ToListAsync(ct);
            return Ok(data, "Almacenes obtenidos correctamente");
        });

        group.MapPost("/warehouses", async (SaveWarehouseRequest request, IValidator<SaveWarehouseRequest> validator, RecepcionDbContext db, CancellationToken ct) =>
        {
            var errors = await ValidateWarehouseAsync(request, null, validator, db, ct);
            if (errors is not null) return errors;

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            if (request.IsDefault) await ClearDefaultAsync(db, request.BranchSalesId, null, ct);
            var warehouse = new Warehouse
            {
                BranchSalesId = request.BranchSalesId,
                Code = request.Code.Trim().ToUpperInvariant(),
                Name = request.Name.Trim(),
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            db.Warehouses.Add(warehouse);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Ok(new { warehouse.Id }, "Almacén registrado correctamente");
        });

        group.MapPut("/warehouses/{id:int}", async (int id, SaveWarehouseRequest request, IValidator<SaveWarehouseRequest> validator, RecepcionDbContext db, CancellationToken ct) =>
        {
            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, ct);
            if (warehouse is null) return NotFound("Almacén no encontrado.");

            var errors = await ValidateWarehouseAsync(request, id, validator, db, ct);
            if (errors is not null) return errors;

            if (warehouse.BranchSalesId != request.BranchSalesId && await db.ProductStocks.AnyAsync(s => s.WarehouseId == id && s.Quantity != 0, ct))
                return BadRequest("No se puede cambiar de sucursal un almacén que tiene existencias.");
            if (!request.IsActive && await db.ProductStocks.AnyAsync(s => s.WarehouseId == id && s.Quantity != 0, ct))
                return BadRequest("No se puede desactivar un almacén con existencias. Transfiere o ajusta el stock primero.");

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            if (request.IsDefault) await ClearDefaultAsync(db, request.BranchSalesId, id, ct);
            warehouse.BranchSalesId = request.BranchSalesId;
            warehouse.Code = request.Code.Trim().ToUpperInvariant();
            warehouse.Name = request.Name.Trim();
            warehouse.IsDefault = request.IsDefault;
            warehouse.IsActive = request.IsActive;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Ok(new { warehouse.Id }, "Almacén actualizado correctamente");
        });
    }

    private static async Task<IResult?> ValidateWarehouseAsync(SaveWarehouseRequest request, int? id, IValidator<SaveWarehouseRequest> validator, RecepcionDbContext db, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
        if (!await db.BranchSales.AnyAsync(b => b.Id == request.BranchSalesId, ct))
            return BadRequest("La sucursal no existe.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Warehouses.AnyAsync(w => w.Code == code && w.Id != id, ct))
            return BadRequest($"Ya existe un almacén con el código {code}.");
        if (request.IsDefault && !request.IsActive)
            return BadRequest("El almacén por defecto debe estar activo.");
        return null;
    }

    private static async Task ClearDefaultAsync(RecepcionDbContext db, int branchId, int? exceptId, CancellationToken ct)
    {
        var defaults = await db.Warehouses.Where(w => w.BranchSalesId == branchId && w.IsDefault && w.Id != exceptId).ToListAsync(ct);
        defaults.ForEach(w => w.IsDefault = false);
        await db.SaveChangesAsync(ct);
    }

    // ---------------- Movimientos ----------------

    private static void MapMovements(RouteGroupBuilder group)
    {
        group.MapGet("/movements", async (DateOnly? from, DateOnly? to, InventoryMovementType? type, int? warehouseId, int? productId,
            InventoryMovementStatus? status, RecepcionDbContext db, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(InventoryPostingService.NowLima());
            var fromDate = (from ?? today.AddDays(-30)).ToDateTime(TimeOnly.MinValue);
            var toDate = (to ?? today).AddDays(1).ToDateTime(TimeOnly.MinValue);

            var query = db.InventoryMovements.AsNoTracking()
                .Where(m => m.MovementDate >= fromDate && m.MovementDate < toDate);
            if (type.HasValue) query = query.Where(m => m.Type == type.Value);
            if (status.HasValue) query = query.Where(m => m.Status == status.Value);
            if (warehouseId.HasValue) query = query.Where(m => m.WarehouseId == warehouseId.Value || m.TargetWarehouseId == warehouseId.Value);
            if (productId.HasValue) query = query.Where(m => m.Lines.Any(l => l.ProductId == productId.Value));

            var data = await query
                .OrderByDescending(m => m.Id)
                .Take(1000)
                .Select(m => new InventoryMovementListItemDto
                {
                    Id = m.Id,
                    Number = m.Number,
                    Type = m.Type,
                    Status = m.Status,
                    MovementDate = m.MovementDate,
                    WarehouseId = m.WarehouseId,
                    WarehouseName = m.Warehouse != null ? m.Warehouse.Name : string.Empty,
                    TargetWarehouseName = m.TargetWarehouse != null ? m.TargetWarehouse.Name : null,
                    AreaName = m.Area != null ? m.Area.Name : null,
                    Reason = m.Reason,
                    PurchaseInvoiceId = m.PurchaseInvoiceId,
                    PurchaseInvoiceRef = m.PurchaseInvoice != null ? m.PurchaseInvoice.InvoiceSerie + "-" + m.PurchaseInvoice.InvoiceNumber : null,
                    LineCount = m.Lines.Count(l => l.WarehouseId == m.WarehouseId),
                    TotalCost = m.Lines.Where(l => l.WarehouseId == m.WarehouseId).Sum(l => l.TotalCost < 0 ? -l.TotalCost : l.TotalCost)
                })
                .ToListAsync(ct);
            return Ok(data, "Movimientos obtenidos correctamente");
        });

        group.MapGet("/movements/{id:long}", async (long id, IInventoryQueryService queries, CancellationToken ct) =>
        {
            var movement = await queries.GetMovementAsync(id, ct);
            return movement is null ? NotFound("Movimiento no encontrado.") : Ok(movement, "Movimiento obtenido correctamente");
        });

        group.MapPost("/movements", async (CreateInventoryMovementRequest request, IValidator<CreateInventoryMovementRequest> validator,
            IInventoryPostingService posting, IInventoryQueryService queries, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid) return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            if (!ManualTypes.Contains(request.Type))
                return BadRequest("Ese tipo de movimiento no se puede registrar manualmente.");

            try
            {
                var movement = await posting.PostAsync(new InventoryMovementDraft
                {
                    Type = request.Type,
                    MovementDate = InventoryPostingService.NowLima(),
                    WarehouseId = request.WarehouseId,
                    TargetWarehouseId = request.Type == InventoryMovementType.Transfer ? request.TargetWarehouseId : null,
                    AreaId = request.Type == InventoryMovementType.Consumption ? request.AreaId : null,
                    Reason = request.Reason,
                    CreatedByUserId = user.GetUserId(),
                    Lines = request.Lines.Select(l => new InventoryDraftLine(l.ProductId, l.Quantity,
                        request.Type is InventoryMovementType.InitialBalance or InventoryMovementType.AdjustmentIn ? l.UnitCost : null)).ToList()
                }, ct);
                return Ok(await queries.GetMovementAsync(movement.Id, ct), $"Movimiento {movement.Number} registrado correctamente");
            }
            catch (InventoryException ex)
            {
                return BadRequest(ex.Message);
            }
        });

        group.MapPost("/movements/{id:long}/cancel", async (long id, CancelInventoryMovementRequest request, IInventoryPostingService posting,
            RecepcionDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var movement = await db.InventoryMovements.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
            if (movement is null) return NotFound("Movimiento no encontrado.");
            if (movement.Type == InventoryMovementType.PurchaseIn)
                return BadRequest("Este ingreso viene de una compra: anula la compra para revertirlo.");

            try
            {
                var reversal = await posting.ReverseAsync(id, request.Reason ?? string.Empty, user.GetUserId(), ct);
                return Ok(new { reversal.Id, reversal.Number }, $"Movimiento anulado con {reversal.Number}");
            }
            catch (InventoryException ex)
            {
                return BadRequest(ex.Message);
            }
        });
    }

    // ---------------- Stock y kardex ----------------

    private static void MapStock(RouteGroupBuilder group)
    {
        group.MapGet("/stock", async (int? warehouseId, int? categoryId, bool? belowMin, string? search, IInventoryQueryService queries, CancellationToken ct) =>
            Ok(await queries.GetStockAsync(warehouseId, categoryId, belowMin == true, search, ct), "Stock obtenido correctamente"));

        group.MapGet("/stock/product/{productId:int}", async (int productId, IInventoryQueryService queries, CancellationToken ct) =>
            Ok(await queries.GetProductStockAsync(productId, ct), "Stock del producto obtenido correctamente"));

        group.MapGet("/kardex", async (int productId, int warehouseId, DateOnly? from, DateOnly? to, IInventoryQueryService queries, CancellationToken ct) =>
        {
            var (fromDate, toDate) = DefaultRange(from, to);
            if (toDate < fromDate) return BadRequest("La fecha final no puede ser anterior a la inicial.");
            var kardex = await queries.GetKardexAsync(productId, warehouseId, fromDate, toDate, ct);
            return kardex is null ? NotFound("Producto o almacén no encontrado.") : Ok(kardex, "Kardex obtenido correctamente");
        });
    }

    // ---------------- utilidades ----------------

    public static (DateOnly From, DateOnly To) DefaultRange(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(InventoryPostingService.NowLima());
        return (from ?? new DateOnly(today.Year, today.Month, 1), to ?? today);
    }

    public static IResult Ok<T>(T data, string message) => Results.Ok(new ApiResponse<T>
    {
        Success = true,
        Data = data,
        StatusCode = StatusCodes.Status200OK,
        Message = message
    });

    public static IResult BadRequest(string message) => Results.BadRequest(new[] { message });

    public static IResult NotFound(string message) => Results.NotFound(new ApiResponse<object>
    {
        Success = false,
        StatusCode = StatusCodes.Status404NotFound,
        Message = message
    });
}

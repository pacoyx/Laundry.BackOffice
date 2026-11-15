using Microsoft.EntityFrameworkCore;

public record ConsumptionReportRowDto
{
    public int? AreaId { get; init; }
    public string AreaName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? UnitName { get; init; }
    public decimal Quantity { get; init; }
    public decimal TotalCost { get; init; }
}

public static class InventoryReportEndpoints
{
    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void MapInventoryReports(this RouteGroupBuilder group)
    {
        // ---- Kardex ----
        group.MapGet("/kardex/excel", async (int productId, int warehouseId, DateOnly? from, DateOnly? to, IInventoryQueryService queries, CancellationToken ct) =>
        {
            var report = await BuildKardexAsync(productId, warehouseId, from, to, queries, ct);
            return report is null ? InventoryEndpoints.NotFound("Producto o almacén no encontrado.")
                : Results.File(report.Value.Report.ToExcel("Kardex"), ExcelType, $"Kardex_{report.Value.FileSuffix}.xlsx");
        });

        group.MapGet("/kardex/pdf", async (int productId, int warehouseId, DateOnly? from, DateOnly? to, IInventoryQueryService queries,
            ICompanyProfileService company, CancellationToken ct) =>
        {
            var report = await BuildKardexAsync(productId, warehouseId, from, to, queries, ct);
            return report is null ? InventoryEndpoints.NotFound("Producto o almacén no encontrado.")
                : Results.File(report.Value.Report.ToPdf(await company.GetCompanyLogoBytesAsync(ct)), "application/pdf", $"Kardex_{report.Value.FileSuffix}.pdf");
        });

        // ---- Stock valorizado ----
        group.MapGet("/reports/stock-valued/excel", async (int? warehouseId, IInventoryQueryService queries, RecepcionDbContext db, CancellationToken ct) =>
            Results.File((await BuildStockValuedAsync(warehouseId, queries, db, ct)).ToExcel("Stock valorizado"), ExcelType,
                $"StockValorizado_{InventoryPostingService.NowLima():yyyyMMdd}.xlsx"));

        group.MapGet("/reports/stock-valued/pdf", async (int? warehouseId, IInventoryQueryService queries, RecepcionDbContext db,
            ICompanyProfileService company, CancellationToken ct) =>
            Results.File((await BuildStockValuedAsync(warehouseId, queries, db, ct)).ToPdf(await company.GetCompanyLogoBytesAsync(ct)), "application/pdf",
                $"StockValorizado_{InventoryPostingService.NowLima():yyyyMMdd}.pdf"));

        // ---- Consumo interno por área ----
        group.MapGet("/reports/consumption", async (DateOnly? from, DateOnly? to, int? warehouseId, int? areaId, RecepcionDbContext db, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            return InventoryEndpoints.Ok(await GetConsumptionAsync(fromDate, toDate, warehouseId, areaId, db, ct), "Consumo obtenido correctamente");
        });

        group.MapGet("/reports/consumption/excel", async (DateOnly? from, DateOnly? to, int? warehouseId, int? areaId, RecepcionDbContext db, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            var report = await BuildConsumptionAsync(fromDate, toDate, warehouseId, areaId, db, ct);
            return Results.File(report.ToExcel("Consumo"), ExcelType, $"ConsumoInterno_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
        });

        group.MapGet("/reports/consumption/pdf", async (DateOnly? from, DateOnly? to, int? warehouseId, int? areaId, RecepcionDbContext db,
            ICompanyProfileService company, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            var report = await BuildConsumptionAsync(fromDate, toDate, warehouseId, areaId, db, ct);
            return Results.File(report.ToPdf(await company.GetCompanyLogoBytesAsync(ct)), "application/pdf", $"ConsumoInterno_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf");
        });

        // ---- Reposición (bajo mínimo) ----
        group.MapGet("/reports/reorder/excel", async (int? warehouseId, IInventoryQueryService queries, RecepcionDbContext db, CancellationToken ct) =>
        {
            var stock = await queries.GetStockAsync(warehouseId, null, belowMinOnly: true, null, ct);
            var report = new InventoryTableReport
            {
                Title = "PRODUCTOS A REPONER",
                Subtitles = { $"Almacén: {await WarehouseLabelAsync(warehouseId, db, ct)}", "Productos con stock por debajo del mínimo" },
                Columns =
                {
                    new("Código", Width: 1f), new("Producto", Width: 3f), new("Categoría", Width: 1.5f), new("Unidad", Width: 1f),
                    new("Stock actual", ReportColumnKind.Quantity), new("Stock mínimo", ReportColumnKind.Quantity),
                    new("Faltante", ReportColumnKind.Quantity), new("Costo prom.", ReportColumnKind.Money)
                },
                Rows = stock.Select(s => new object?[]
                {
                    s.ProductCode, s.ProductName, s.CategoryName, s.UnitName, s.Quantity, s.MinStock, s.MinStock - s.Quantity, s.AverageCost
                }).ToList()
            };
            return Results.File(report.ToExcel("Reposición"), ExcelType, $"Reposicion_{InventoryPostingService.NowLima():yyyyMMdd}.xlsx");
        });
    }

    private static async Task<(InventoryTableReport Report, string FileSuffix)?> BuildKardexAsync(int productId, int warehouseId,
        DateOnly? from, DateOnly? to, IInventoryQueryService queries, CancellationToken ct)
    {
        var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
        var k = await queries.GetKardexAsync(productId, warehouseId, fromDate, toDate, ct);
        if (k is null) return null;

        var rows = new List<object?[]>
        {
            new object?[] { null, null, "Saldo inicial", null, null, null, null, k.OpeningQuantity, k.OpeningAverageCost, k.OpeningValue }
        };
        rows.AddRange(k.Lines.Select(l => new object?[]
        {
            l.MovementDate, l.MovementNumber,
            l.Status == InventoryMovementStatus.Cancelled ? $"{l.Detail} (anulado)" : l.Detail,
            l.QuantityIn == 0 ? null : l.QuantityIn, l.QuantityOut == 0 ? null : l.QuantityOut,
            l.UnitCost, l.TotalCost, l.BalanceQuantity, l.BalanceAverageCost, l.BalanceValue
        }));

        var report = new InventoryTableReport
        {
            Title = "KARDEX VALORIZADO",
            Subtitles =
            {
                $"Producto: {(k.ProductCode is null ? "" : k.ProductCode + " - ")}{k.ProductName}{(k.UnitName is null ? "" : $" ({k.UnitName})")}",
                $"Almacén: {k.WarehouseName}   |   Del {k.From:dd/MM/yyyy} al {k.To:dd/MM/yyyy}   |   Método: costo promedio ponderado"
            },
            Columns =
            {
                new("Fecha", ReportColumnKind.Date, 1.3f), new("Documento", Width: 1.2f), new("Detalle", Width: 3f),
                new("Entrada", ReportColumnKind.Quantity), new("Salida", ReportColumnKind.Quantity),
                new("Costo unit.", ReportColumnKind.Money), new("Costo total", ReportColumnKind.Money),
                new("Saldo", ReportColumnKind.Quantity), new("Costo prom.", ReportColumnKind.Money), new("Valor saldo", ReportColumnKind.Money)
            },
            Rows = rows,
            Totals = new object?[] { null, null, "Totales del período", k.TotalIn, k.TotalOut, null, null, k.ClosingQuantity, k.ClosingAverageCost, k.ClosingValue }
        };
        return (report, $"{k.ProductCode ?? k.ProductId.ToString()}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}");
    }

    private static async Task<InventoryTableReport> BuildStockValuedAsync(int? warehouseId, IInventoryQueryService queries, RecepcionDbContext db, CancellationToken ct)
    {
        var stock = (await queries.GetStockAsync(warehouseId, null, false, null, ct)).Where(s => s.Quantity != 0).ToList();
        return new InventoryTableReport
        {
            Title = "STOCK VALORIZADO",
            Subtitles = { $"Almacén: {await WarehouseLabelAsync(warehouseId, db, ct)}   |   Al {InventoryPostingService.NowLima():dd/MM/yyyy HH:mm}   |   Costo promedio ponderado, sin IGV" },
            Columns =
            {
                new("Código", Width: 1f), new("Producto", Width: 3f), new("Categoría", Width: 1.5f), new("Unidad", Width: 1f),
                new("Cantidad", ReportColumnKind.Quantity), new("Costo prom.", ReportColumnKind.Money), new("Valor total", ReportColumnKind.Money)
            },
            Rows = stock.Select(s => new object?[] { s.ProductCode, s.ProductName, s.CategoryName, s.UnitName, s.Quantity, s.AverageCost, s.TotalValue }).ToList(),
            Totals = new object?[] { null, $"{stock.Count} producto(s)", null, null, null, null, stock.Sum(s => s.TotalValue) }
        };
    }

    private static async Task<List<ConsumptionReportRowDto>> GetConsumptionAsync(DateOnly from, DateOnly to, int? warehouseId, int? areaId,
        RecepcionDbContext db, CancellationToken ct)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // Solo consumos vigentes: los anulados quedan con Status = Cancelled.
        var query = db.InventoryMovementLines.AsNoTracking()
            .Where(l => l.InventoryMovement!.Type == InventoryMovementType.Consumption
                && l.InventoryMovement.Status == InventoryMovementStatus.Posted
                && l.InventoryMovement.MovementDate >= fromDate && l.InventoryMovement.MovementDate < toDate);
        if (warehouseId.HasValue) query = query.Where(l => l.WarehouseId == warehouseId.Value);
        if (areaId.HasValue) query = query.Where(l => l.InventoryMovement!.AreaId == areaId.Value);

        var rows = await query
            .GroupBy(l => new
            {
                l.InventoryMovement!.AreaId,
                AreaName = l.InventoryMovement.Area != null ? l.InventoryMovement.Area.Name : "(sin área)",
                l.ProductId,
                l.Product!.Code,
                l.Product.Name,
                UnitName = l.Product.UnitMeasurement != null ? l.Product.UnitMeasurement.Name : null
            })
            .Select(g => new ConsumptionReportRowDto
            {
                AreaId = g.Key.AreaId,
                AreaName = g.Key.AreaName,
                ProductId = g.Key.ProductId,
                ProductCode = g.Key.Code,
                ProductName = g.Key.Name,
                UnitName = g.Key.UnitName,
                Quantity = -g.Sum(l => l.Quantity),
                TotalCost = -g.Sum(l => l.TotalCost)
            })
            .ToListAsync(ct);

        return rows.OrderBy(r => r.AreaName).ThenByDescending(r => r.TotalCost).ToList();
    }

    private static async Task<InventoryTableReport> BuildConsumptionAsync(DateOnly from, DateOnly to, int? warehouseId, int? areaId,
        RecepcionDbContext db, CancellationToken ct)
    {
        var rows = await GetConsumptionAsync(from, to, warehouseId, areaId, db, ct);
        var areaLabel = areaId.HasValue
            ? await db.Areas.Where(a => a.Id == areaId.Value).Select(a => a.Name).FirstOrDefaultAsync(ct) ?? "-"
            : "Todas";
        return new InventoryTableReport
        {
            Title = "CONSUMO INTERNO POR ÁREA",
            Subtitles = { $"Del {from:dd/MM/yyyy} al {to:dd/MM/yyyy}   |   Almacén: {await WarehouseLabelAsync(warehouseId, db, ct)}   |   Área: {areaLabel}" },
            Columns =
            {
                new("Área", Width: 1.5f), new("Código", Width: 1f), new("Producto", Width: 3f), new("Unidad", Width: 1f),
                new("Cantidad", ReportColumnKind.Quantity), new("Costo total", ReportColumnKind.Money)
            },
            Rows = rows.Select(r => new object?[] { r.AreaName, r.ProductCode, r.ProductName, r.UnitName, r.Quantity, Math.Round(r.TotalCost, 2) }).ToList(),
            Totals = new object?[] { "Total", null, null, null, null, Math.Round(rows.Sum(r => r.TotalCost), 2) },
            Landscape = false
        };
    }

    private static async Task<string> WarehouseLabelAsync(int? warehouseId, RecepcionDbContext db, CancellationToken ct) =>
        warehouseId.HasValue
            ? await db.Warehouses.Where(w => w.Id == warehouseId.Value).Select(w => w.Name).FirstOrDefaultAsync(ct) ?? "-"
            : "Todos los almacenes";
}

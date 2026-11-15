using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public class CreateSaleValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.PaymentType)
            .Must(p => SalePaymentTypes.All.Contains(p))
            .WithMessage("Seleccione el medio de pago (efectivo, tarjeta o Yape/Plin).");
        RuleFor(x => x.AmountReceived).GreaterThanOrEqualTo(0).WithMessage("El monto recibido no puede ser negativo.");
        RuleFor(x => x.Notes).MaximumLength(300).WithMessage("La observación no puede superar los 300 caracteres.");
        RuleFor(x => x.PaymentDescription).MaximumLength(50).WithMessage("El detalle del pago no puede superar los 50 caracteres.");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Agregue al menos un producto.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0).WithMessage("Hay una línea sin producto.");
            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Las cantidades deben ser mayores a cero.")
                .PrecisionScale(18, 4, true).WithMessage("La cantidad admite como máximo 4 decimales.");
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
        });
    }
}

public static class SalesEndpoints
{
    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void MapSales(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales").WithTags("Sales").RequirePermission(PermissionCodes.Sales);

        group.MapGet("/pos-context", async (ISaleService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (userId is null) return Results.Unauthorized();
            try
            {
                return InventoryEndpoints.Ok(await service.GetPosContextAsync(userId.Value, ct), "Contexto obtenido correctamente");
            }
            catch (SaleException ex)
            {
                return InventoryEndpoints.BadRequest(ex.Message);
            }
        });

        group.MapGet("/products", async (string? q, int? warehouseId, ISaleService service, CancellationToken ct) =>
            InventoryEndpoints.Ok(await service.SearchProductsAsync(q, warehouseId, ct), "Productos obtenidos correctamente"));

        group.MapPost("/", async (CreateSaleRequest request, IValidator<CreateSaleRequest> validator, ISaleService service,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid) return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            var userId = user.GetUserId();
            if (userId is null) return Results.Unauthorized();

            try
            {
                var id = await service.CreateAsync(request, userId.Value, ct);
                var sale = await service.GetAsync(id, ct);
                return InventoryEndpoints.Ok(sale, $"Venta {sale?.Number} registrada correctamente");
            }
            catch (SaleException ex)
            {
                return InventoryEndpoints.BadRequest(ex.Message);
            }
        });

        group.MapGet("/", async (DateOnly? from, DateOnly? to, SaleStatus? status, string? paymentType, int? userId,
            RecepcionDbContext db, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            return InventoryEndpoints.Ok(await QueryAsync(db, fromDate, toDate, status, paymentType, userId, ct), "Ventas obtenidas correctamente");
        });

        group.MapGet("/{id:long}", async (long id, ISaleService service, CancellationToken ct) =>
        {
            var sale = await service.GetAsync(id, ct);
            return sale is null ? InventoryEndpoints.NotFound("Venta no encontrada.") : InventoryEndpoints.Ok(sale, "Venta obtenida correctamente");
        });

        group.MapPost("/{id:long}/cancel", async (long id, CancelSaleRequest request, ISaleService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (userId is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(request.Reason)) return InventoryEndpoints.BadRequest("Indique el motivo de la anulación.");
            try
            {
                await service.CancelAsync(id, request.Reason, userId.Value, ct);
                return InventoryEndpoints.Ok(new { id }, "Venta anulada correctamente");
            }
            catch (SaleException ex)
            {
                return InventoryEndpoints.BadRequest(ex.Message);
            }
        });

        // ---- Reporte de ventas del período ----
        group.MapGet("/reports/sales/excel", async (DateOnly? from, DateOnly? to, RecepcionDbContext db, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            var report = await BuildReportAsync(db, fromDate, toDate, ct);
            return Results.File(report.ToExcel("Ventas"), ExcelType, $"Ventas_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
        });

        group.MapGet("/reports/sales/pdf", async (DateOnly? from, DateOnly? to, RecepcionDbContext db, ICompanyProfileService company, CancellationToken ct) =>
        {
            var (fromDate, toDate) = InventoryEndpoints.DefaultRange(from, to);
            var report = await BuildReportAsync(db, fromDate, toDate, ct);
            return Results.File(report.ToPdf(await company.GetCompanyLogoBytesAsync(ct)), "application/pdf", $"Ventas_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf");
        });
    }

    private static async Task<List<SaleListItemDto>> QueryAsync(RecepcionDbContext db, DateOnly from, DateOnly to, SaleStatus? status,
        string? paymentType, int? userId, CancellationToken ct)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var query = db.Sales.AsNoTracking().Where(s => s.SaleDate >= fromDate && s.SaleDate < toDate);
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(paymentType)) query = query.Where(s => s.PaymentType == paymentType);
        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

        return await query
            .OrderByDescending(s => s.Id)
            .Take(2000)
            .Select(s => new SaleListItemDto
            {
                Id = s.Id,
                Number = s.Number,
                SaleDate = s.SaleDate,
                CustomerName = s.CustomerName,
                BranchName = s.BranchSales != null ? s.BranchSales.Description : string.Empty,
                UserName = db.Users.Where(u => u.Id == s.UserId).Select(u => u.UserName).FirstOrDefault() ?? string.Empty,
                PaymentType = s.PaymentType,
                Total = s.Total,
                Status = s.Status,
                ItemCount = s.Details.Count
            })
            .ToListAsync(ct);
    }

    private static async Task<InventoryTableReport> BuildReportAsync(RecepcionDbContext db, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var sales = await db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= fromDate && s.SaleDate < toDate)
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Number, s.SaleDate, s.CustomerName, s.PaymentType, s.Subtotal, s.Igv, s.Total, s.Status,
                Cost = s.Details.Sum(d => d.Quantity * d.UnitCost)
            })
            .ToListAsync(ct);

        var valid = sales.Where(s => s.Status == SaleStatus.Completed).ToList();
        var byPayment = string.Join("   |   ", SalePaymentTypes.All
            .Select(p => $"{SalePaymentTypes.Label(p)}: {valid.Where(s => s.PaymentType == p).Sum(s => s.Total):N2}"));

        return new InventoryTableReport
        {
            Title = "REPORTE DE VENTAS",
            Subtitles =
            {
                $"Del {from:dd/MM/yyyy} al {to:dd/MM/yyyy}   |   {valid.Count} venta(s) vigente(s), {sales.Count - valid.Count} anulada(s)",
                $"Por medio de pago (vigentes) →   {byPayment}"
            },
            Columns =
            {
                new("Fecha", ReportColumnKind.Date, 1.3f), new("N° venta", Width: 1.3f), new("Cliente", Width: 2.2f), new("Pago", Width: 1f),
                new("Subtotal", ReportColumnKind.Money), new("IGV", ReportColumnKind.Money), new("Total", ReportColumnKind.Money),
                new("Costo", ReportColumnKind.Money), new("Margen", ReportColumnKind.Money), new("Estado", Width: 1f)
            },
            Rows = sales.Select(s => new object?[]
            {
                s.SaleDate, s.Number, s.CustomerName, SalePaymentTypes.Label(s.PaymentType), s.Subtotal, s.Igv, s.Total,
                Math.Round(s.Cost, 2),
                // Margen sobre el valor sin IGV (el costo también es sin IGV).
                s.Status == SaleStatus.Completed ? Math.Round(s.Subtotal - s.Cost, 2) : null,
                s.Status == SaleStatus.Completed ? "Vigente" : "Anulada"
            }).ToList(),
            Totals = new object?[]
            {
                null, null, "Total vigentes", null, valid.Sum(s => s.Subtotal), valid.Sum(s => s.Igv), valid.Sum(s => s.Total),
                Math.Round(valid.Sum(s => s.Cost), 2), Math.Round(valid.Sum(s => s.Subtotal - s.Cost), 2), null
            }
        };
    }
}

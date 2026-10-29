using Microsoft.EntityFrameworkCore;

public static class GetBranchesSalesEndpoint
{
    public static void MapGetBranchesSales(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext context) =>
        {
            var branchesSales = await context.BranchSales
                .AsNoTracking()
                .GroupJoin(
                    context.Printers.AsNoTracking(),
                    bs => bs.DefaultPrinterId,
                    p => p.Id,
                    (bs, printers) => new { bs, printers })
                .SelectMany(
                    x => x.printers.DefaultIfEmpty(),
                    (x, printer) => new BranchSalesComboResponse(
                        x.bs.Id,
                        x.bs.Description,
                        x.bs.Address,
                        x.bs.Status,
                        x.bs.Code,
                        x.bs.DefaultPrinterId,
                        printer != null ? printer.WindowsPrinterName : null,
                        x.bs.IsCentral
                    ))
                .ToListAsync();

            var response = new ApiResponse<List<BranchSalesComboResponse>>
            {
                Data = branchesSales,
                Message = "Lista de sucursales de ventas",
                StatusCode = 200,
                Success = true
            };

            return Results.Ok(response);
        });
    }
}

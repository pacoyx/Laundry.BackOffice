using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.PrinterHub;

public static class GetPrinterConnectionStatusEndpoint
{
    public static void MapGetPrinterConnectionStatus(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connection-status", async (int branchId, RecepcionDbContext db, PrinterRegistry registry) =>
        {
            var branch = await db.BranchSales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == branchId);
            if (branch is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Sucursal no encontrada.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var printer = await db.Printers
                .AsNoTracking()
                .Include(x => x.WorkerNode)
                .FirstOrDefaultAsync(x => x.Id == branch.DefaultPrinterId);

            if (printer is null)
            {
                return Results.Ok(new ApiResponse<PrinterConnectionStatusResponse>
                {
                    Success = true,
                    Message = "La sucursal no tiene una impresora configurada.",
                    StatusCode = StatusCodes.Status200OK,
                    Data = new PrinterConnectionStatusResponse(
                        false, null, null, null,
                        "La sucursal no tiene una impresora por defecto configurada.")
                });
            }

            // Chequeo instantaneo contra el registro en memoria (sin imprimir nada): resuelve el
            // caso mas comun -el worker simplemente no esta conectado- sin gastar papel.
            var isConnected = registry.IsConnected(printer.WorkerNodeId);

            return Results.Ok(new ApiResponse<PrinterConnectionStatusResponse>
            {
                Success = true,
                Message = isConnected ? "Impresora conectada." : "Impresora desconectada.",
                StatusCode = StatusCodes.Status200OK,
                Data = new PrinterConnectionStatusResponse(
                    isConnected,
                    printer.Id,
                    printer.WindowsPrinterName,
                    printer.WorkerNode.LastConnectionAt,
                    isConnected ? null : "El worker de impresión no está conectado. Si el problema persiste, reinicia el servicio en la PC de recepción.")
            });
        })
        .WithName("GetPrinterConnectionStatus")
        .Produces<ApiResponse<PrinterConnectionStatusResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

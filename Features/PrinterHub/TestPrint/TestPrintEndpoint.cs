using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.PrinterHub;

public static class TestPrintEndpoint
{
    public static void MapTestPrint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/test-print", async (
            int printerId,
            RecepcionDbContext db,
            PrinterRegistry registry,
            IHubContext<PrintHub> hubContext) =>
        {
            var printer = await db.Printers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == printerId);

            if (printer is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Impresora no encontrada.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            if (!registry.IsConnected(printer.WorkerNodeId))
            {
                return Results.BadRequest(new[] { "El worker de impresión no está conectado. No se puede enviar la prueba." });
            }

            // No se crea fila en PrintQueue a propósito: una prueba no es un documento de
            // negocio real, no necesita reintentos ni quedar en el historial de impresiones.
            await hubContext.Clients
                .Group($"worker-{printer.WorkerNodeId}")
                .SendAsync("PrintTestTicket", new TestPrintCommandDto(printer.WindowsPrinterName, DateTime.Now));

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Comando de prueba enviado. Revisa la impresora.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { printer.Id, printer.WindowsPrinterName }
            });
        })
        .WithName("TestPrint")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

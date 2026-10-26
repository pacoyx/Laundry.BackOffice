using _24dex_backend_comercial.Features.WorkGuideMain.CreateWorkGuideMain;
using DexterCompany.Models;
using Microsoft.EntityFrameworkCore;

public static class ReprintWorkGuideMainEndpoint
{
    public static void MapReprintWorkGuideMain(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id}/reprint", async (
            int id,
            ReprintWorkGuideMainRequestDto request,
            RecepcionDbContext db,
            IAppLogger<string> logger,
            IPrintNotifier printNotifier) =>
        {
            var workGuide = await db.WorkGuideMains.FindAsync(id);
            if (workGuide == null)
            {
                logger.LogWarning("Guia no encontrada " + id, "ReprintWorkGuideMainEndpoint");
                var notFoundResponse = new ApiResponse<string>
                {
                    Data = "",
                    Message = "Guia no encontrada",
                    Success = false,
                    StatusCode = 404
                };
                return Results.NotFound(notFoundResponse);
            }

            // reutiliza el ultimo trabajo de impresion de la guia, si existe
            var printQueue = await db.PrintQueues
                .Where(x => x.WorkGuideId == id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (printQueue == null)
            {
                var branch = await db.BranchSales.SingleOrDefaultAsync(x => x.Id == workGuide.BranchId);
                if (branch == null)
                {
                    logger.LogWarning("No tiene impresora configurada por defecto esta sucursal", "ReprintWorkGuideMainEndpoint");
                    var branchErrResponse = new ApiResponse<string>
                    {
                        Data = "",
                        Message = "No tiene impresora configurada por defecto esta sucursal",
                        Success = false,
                        StatusCode = 400
                    };
                    return Results.BadRequest(branchErrResponse);
                }

                var numbersDocument = await db.NumbersDocuments
                    .FirstOrDefaultAsync(nd => nd.BranchId == workGuide.BranchId && nd.TypeProcess == "RECEP");
                if (numbersDocument == null)
                {
                    logger.LogWarning("No se encontro correlativo de guia de servicio", "ReprintWorkGuideMainEndpoint");
                    var docErrResponse = new ApiResponse<string>
                    {
                        Data = "",
                        Message = "No se encontro correlativo de guia de servicio",
                        Success = false,
                        StatusCode = 400
                    };
                    return Results.BadRequest(docErrResponse);
                }

                printQueue = new PrintQueue
                {
                    WorkGuideId = workGuide.Id,
                    PrinterId = branch.DefaultPrinterId,
                    Status = PrintStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    DocumentType = numbersDocument.TypeDoc,
                    PrintCopy = request.PrintCopy,
                };
                db.PrintQueues.Add(printQueue);
            }
            else
            {
                printQueue.Status = PrintStatus.Pending;
                printQueue.RetryCount++;
                printQueue.ErrorMessage = null;
                printQueue.ProcessedAt = null;
                printQueue.PrintCopy = request.PrintCopy;
            }

            await db.SaveChangesAsync();
            await printNotifier.NotifyAsync(printQueue.PrinterId, printQueue.Id);

            logger.LogInformacion($"Guia {workGuide.SerieGuia}-{workGuide.NumeroGuia} enviada a reimprimir (printQueueId: {printQueue.Id})");

            var response = new ApiResponse<long>
            {
                Data = printQueue.Id,
                Message = "Guia enviada a reimprimir",
                Success = true,
                StatusCode = 200
            };

            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionEmision);
    }
}

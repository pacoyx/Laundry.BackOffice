using Microsoft.EntityFrameworkCore;

public static class GetPrintJobEndpoint
{
    public static void MapGetPrintJob(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/printjobs/{id:long}",
            async (
                long id,
                RecepcionDbContext db) =>
            {
                var printJob =
                    await db.PrintQueues
                        .Include(x => x.WorkGuideMain)
                            .ThenInclude(w => w.Customer)
                        .Include(x => x.WorkGuideMain)
                            .ThenInclude(w => w.WorkGuideDetails)
                                .ThenInclude(d => d.Product)
                        .Include(x => x.WorkGuideMain)
                            .ThenInclude(w => w.User)
                        .FirstOrDefaultAsync(x =>
                            x.Id == id);

                if (printJob is null)
                {
                    return Results.NotFound();
                }

                var workGuide = printJob.WorkGuideMain;
                var customer = printJob.WorkGuideMain.Customer;

                var response = new PrintJobResponse(
                    printJob.Id,
                    workGuide.Id,
                    workGuide.SerieGuia,
                    workGuide.NumeroGuia,
                    workGuide.FechaOperacion,
                    workGuide.FechaHoraEntrega,
                    workGuide.Customer != null
                        ? $"{workGuide.Customer.FirtsName} {workGuide.Customer.LastName}"
                        : string.Empty,
                    workGuide.Customer!.Phone!,
                    workGuide.Customer!.Address!,
                    workGuide.TipoPago,
                    workGuide.DescripcionPago,
                    workGuide.Total,
                    workGuide.Acuenta,
                    workGuide.Saldo,
                    workGuide.Observaciones,
                    workGuide.MensajeAlertas,
                    printJob.PrinterId,
                    workGuide.User != null ? workGuide.User.UserName : string.Empty,
                    workGuide.TipoRecepcion =="D",
                    printJob.PrintCopy > 0,
                    workGuide.WorkGuideDetails.Select(d => new PrintJobDetailResponse(
                        d.ProductId,
                        d.Product != null ? d.Product.Name : string.Empty,
                        d.Cant,
                        d.Precio,
                        d.Total,
                        d.Observaciones,
                        d.Ubicacion,
                        d.Identificador
                    )).ToList()
                );

                return Results.Ok(response);
            });
    }
}

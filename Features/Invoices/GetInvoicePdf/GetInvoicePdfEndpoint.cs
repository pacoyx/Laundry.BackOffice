public static class GetInvoicePdfEndpoint
{
    public static void MapGetInvoicePdf(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}/pdf", async (
            long id,
            IInvoicePdfService pdfService,
            CancellationToken cancellationToken) =>
        {
            byte[] pdfBytes;
            try
            {
                pdfBytes = await pdfService.GenerateAsync(id, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound("Factura no encontrada.");
            }

            var fileName = $"Factura_{id}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithName("GetInvoicePdf")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

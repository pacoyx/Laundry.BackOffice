public static class GetGuiaRemisionPdfEndpoint
{
    public static void MapGetGuiaRemisionPdf(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}/pdf", async (
            long id,
            IGuiaRemisionPdfService pdfService,
            CancellationToken cancellationToken) =>
        {
            byte[] pdfBytes;
            try
            {
                pdfBytes = await pdfService.GenerateAsync(id, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound("Guía de remisión no encontrada.");
            }

            var fileName = $"GuiaRemision_{id}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithName("GetGuiaRemisionPdf")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

using Microsoft.EntityFrameworkCore;

public static class GetGuiaRemisionByInvoiceEndpoint
{
    public static void MapGetGuiaRemisionByInvoice(this IEndpointRouteBuilder app)
    {
        app.MapGet("/by-invoice/{invoiceId:long}", async (long invoiceId, RecepcionDbContext db) =>
        {
            var guia = await db.GuiasRemision
                .AsNoTracking()
                .Include(x => x.Invoice).ThenInclude(i => i.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.Vehicle)
                .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);

            if (guia is null)
            {
                return Results.NotFound("Esta factura no tiene una guía de remisión registrada.");
            }

            return Results.Ok(new ApiResponse<GuiaRemisionResponse>
            {
                Success = true,
                Message = "Guía de remisión obtenida correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = GuiasRemisionEndpoints.ToResponse(guia)
            });
        })
        .WithName("GetGuiaRemisionByInvoice")
        .Produces<ApiResponse<GuiaRemisionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

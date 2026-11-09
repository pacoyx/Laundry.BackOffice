using Microsoft.EntityFrameworkCore;

public static class GetPickupMobileGarmentTicketsAggregateEndpoint
{
    public static void MapGetPickupMobileGarmentTicketsAggregate(this IEndpointRouteBuilder app)
    {
        app.MapPost("/garment-tickets/aggregate", async (
            PickupMobileGarmentTicketsAggregateRequest request,
            RecepcionDbContext db) =>
        {
            if (request.TicketIds is not { Count: > 0 })
            {
                return Results.BadRequest(new[] { "Debe indicar al menos un ticket." });
            }

            // Solo agrega tickets pendientes: uno ya recogido o anulado no debe poder
            // volver a prellenar una guía.
            var details = await db.GarmentIntakeTicketDetails
                .AsNoTracking()
                .Where(d => request.TicketIds.Contains(d.TicketId) && d.Ticket.Status == GarmentIntakeTicketStatus.Pending)
                .GroupBy(d => d.ServiceItemId)
                .Select(g => new PickupGuideDetailRequest(g.Key, g.Sum(x => x.Quantity), null, null))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<PickupGuideDetailRequest>>
            {
                Success = true,
                Message = "Detalle agregado de los tickets seleccionados.",
                StatusCode = StatusCodes.Status200OK,
                Data = details
            });
        })
        .WithName("GetPickupMobileGarmentTicketsAggregate")
        .Produces<ApiResponse<List<PickupGuideDetailRequest>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

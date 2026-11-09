using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetPickupMobilePendingGarmentTicketsEndpoint
{
    public static void MapGetPickupMobilePendingGarmentTickets(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customers/{customerId:int}/garment-tickets/pending", async (
            int customerId,
            [FromQuery] string? ticketNumber,
            RecepcionDbContext db) =>
        {
            var query = db.GarmentIntakeTickets
                .AsNoTracking()
                .Include(x => x.Details)
                .Where(x => x.CustomerId == customerId && x.Status == GarmentIntakeTicketStatus.Pending);

            if (!string.IsNullOrWhiteSpace(ticketNumber))
            {
                query = query.Where(x => x.TicketNumber.Contains(ticketNumber));
            }

            var tickets = await query
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            var response = tickets.Select(GarmentTicketsEndpoints.ToSummaryResponse).ToList();

            return Results.Ok(new ApiResponse<List<GarmentIntakeTicketSummaryResponse>>
            {
                Success = true,
                Message = "Tickets pendientes de recojo del cliente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetPickupMobilePendingGarmentTickets")
        .Produces<ApiResponse<List<GarmentIntakeTicketSummaryResponse>>>(StatusCodes.Status200OK);
    }
}

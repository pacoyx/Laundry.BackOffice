using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetGarmentTicketsEndpoint
{
    public static void MapGetGarmentTickets(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garment-tickets", async (
            ClaimsPrincipal principal,
            [FromQuery] GarmentIntakeTicketStatus? status,
            [FromQuery] string? ticketNumber,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var query = db.GarmentIntakeTickets
                .AsNoTracking()
                .Include(x => x.Details)
                .Where(x => x.CustomerId == customerId.Value);

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

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
                Message = "Listado de tickets de ingreso de prendas.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetGarmentTickets")
        .Produces<ApiResponse<List<GarmentIntakeTicketSummaryResponse>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

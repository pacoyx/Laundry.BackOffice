using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetGarmentTicketEndpoint
{
    public static void MapGetGarmentTicket(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garment-tickets/{id:int}", async (int id, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var ticket = await db.GarmentIntakeTickets
                .AsNoTracking()
                .Include(x => x.Details).ThenInclude(d => d.Worker)
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .Include(x => x.Photos)
                .Include(x => x.CreatedByCustomerUser)
                .Include(x => x.VoidedByCustomerUser)
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (ticket is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new ApiResponse<GarmentIntakeTicketResponse>
            {
                Success = true,
                Message = "Detalle del ticket de ingreso de prendas.",
                StatusCode = StatusCodes.Status200OK,
                Data = GarmentTicketsEndpoints.ToResponse(ticket)
            });
        })
        .WithName("GetGarmentTicket")
        .Produces<ApiResponse<GarmentIntakeTicketResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

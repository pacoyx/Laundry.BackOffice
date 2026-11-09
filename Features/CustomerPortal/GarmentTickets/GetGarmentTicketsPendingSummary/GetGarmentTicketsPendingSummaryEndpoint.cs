using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetGarmentTicketsPendingSummaryEndpoint
{
    public static void MapGetGarmentTicketsPendingSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garment-tickets/pending-summary", async (ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Ordenar ANTES de proyectar al record GarmentIntakeTicketPendingSummaryItem: EF Core no puede
            // traducir un OrderBy sobre una propiedad de un objeto ya construido por el Select
            // (tira InvalidOperationException en runtime, no en compilacion).
            var summary = await db.GarmentIntakeTicketDetails
                .AsNoTracking()
                .Where(d => d.Ticket.CustomerId == customerId.Value && d.Ticket.Status == GarmentIntakeTicketStatus.Pending)
                .GroupBy(d => new { d.ServiceItemId, d.ServiceItem.Name })
                .OrderBy(g => g.Key.Name)
                .Select(g => new GarmentIntakeTicketPendingSummaryItem(g.Key.ServiceItemId, g.Key.Name, g.Sum(x => x.Quantity)))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<GarmentIntakeTicketPendingSummaryItem>>
            {
                Success = true,
                Message = "Resumen de prendas pendientes de recojo, agrupado por servicio.",
                StatusCode = StatusCodes.Status200OK,
                Data = summary
            });
        })
        .WithName("GetGarmentTicketsPendingSummary")
        .Produces<ApiResponse<List<GarmentIntakeTicketPendingSummaryItem>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

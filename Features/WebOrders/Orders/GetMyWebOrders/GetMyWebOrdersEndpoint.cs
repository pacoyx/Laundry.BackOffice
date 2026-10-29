using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetMyWebOrdersEndpoint
{
    public static void MapGetMyWebOrders(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetWebCustomerCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var orders = await db.WebOrders
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new WebOrderSummaryResponse(x.Id, x.OrderNumber, x.Status, x.Total, x.CreatedAt, x.ScheduledPickupDate))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<WebOrderSummaryResponse>>
            {
                Success = true,
                Message = "Mis pedidos.",
                StatusCode = StatusCodes.Status200OK,
                Data = orders
            });
        })
        .WithName("GetMyWebOrders")
        .WithTags("WebOrders")
        .Produces<ApiResponse<List<WebOrderSummaryResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

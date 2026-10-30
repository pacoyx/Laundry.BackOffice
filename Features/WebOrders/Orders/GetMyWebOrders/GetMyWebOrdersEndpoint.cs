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
                .Include(x => x.WorkGuideMain)
                .Where(x => x.CustomerId == customerId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new WebOrderSummaryResponse(
                    x.Id,
                    x.OrderNumber,
                    x.Status,
                    // Una vez generada la guia, el total real puede cambiar (items/pesos ajustados
                    // por el personal); se refleja aqui en vez de quedar con el total original del pedido.
                    x.WorkGuideMain != null ? x.WorkGuideMain.Total : x.Total,
                    x.CreatedAt,
                    x.ScheduledPickupDate))
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

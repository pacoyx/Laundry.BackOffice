using Microsoft.EntityFrameworkCore;

public static class GetWebOrdersAdminEndpoint
{
    public static void MapGetWebOrdersAdmin(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db, WebOrderStatus? status) =>
        {
            var query = db.WebOrders
                .AsNoTracking()
                .Include(x => x.Customer)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new WebOrderAdminSummaryResponse(
                    x.Id,
                    x.OrderNumber,
                    x.Status,
                    x.Customer.FirtsName + " " + x.Customer.LastName,
                    x.ContactPhone,
                    x.DeliveryAddress,
                    x.Total,
                    x.CreatedAt,
                    x.RequestedPickupDate,
                    x.ScheduledPickupDate,
                    x.AdditionalItemsNote))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<WebOrderAdminSummaryResponse>>
            {
                Success = true,
                Message = "Pedidos web.",
                StatusCode = StatusCodes.Status200OK,
                Data = orders
            });
        })
        .WithName("GetWebOrdersAdmin")
        .WithTags("WebOrders")
        .Produces<ApiResponse<List<WebOrderAdminSummaryResponse>>>(StatusCodes.Status200OK)
        .RequirePermission(PermissionCodes.ReceptionPedidosWeb);
    }
}

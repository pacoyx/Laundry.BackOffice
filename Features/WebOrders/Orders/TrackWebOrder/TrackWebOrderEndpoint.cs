public static class TrackWebOrderEndpoint
{
    public static void MapTrackWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapGet("/track", async (string orderNumber, string phone, RecepcionDbContext db, CancellationToken cancellationToken) =>
        {
            var webOrder = await WebOrderTrackingResolver.FindAsync(db, orderNumber, phone, cancellationToken);

            if (webOrder is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se encontró un pedido con ese número y teléfono.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var response = new WebOrderTrackResponse(
                webOrder.OrderNumber,
                webOrder.Status,
                webOrder.ScheduledPickupDate,
                webOrder.CreatedAt);

            return Results.Ok(new ApiResponse<WebOrderTrackResponse>
            {
                Success = true,
                Message = "Estado del pedido.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("TrackWebOrder")
        .WithTags("WebOrders")
        .Produces<ApiResponse<WebOrderTrackResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireRateLimiting("web-order-track")
        .AllowAnonymous();
    }
}

using Microsoft.EntityFrameworkCore;

public static class RejectWebOrderEndpoint
{
    public static void MapRejectWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:long}/reject", async (long id, RejectWebOrderRequest request, RecepcionDbContext db) =>
        {
            var webOrder = await db.WebOrders.FirstOrDefaultAsync(x => x.Id == id);
            if (webOrder is null)
            {
                return Results.NotFound();
            }

            if (webOrder.Status != WebOrderStatus.Pendiente)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Solo se pueden rechazar pedidos pendientes.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            webOrder.Status = WebOrderStatus.Rechazado;
            webOrder.PickupObservation = request.Reason;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Pedido rechazado.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("RejectWebOrder")
        .WithTags("WebOrders")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionPedidosWeb);
    }
}

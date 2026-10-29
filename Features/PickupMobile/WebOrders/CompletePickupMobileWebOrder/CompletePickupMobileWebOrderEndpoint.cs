using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class CompletePickupMobileWebOrderEndpoint
{
    public static void MapCompletePickupMobileWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapPut("/my-web-orders/{id:long}/complete", async (
            long id,
            CompletePickupMobileWebOrderRequest request,
            ClaimsPrincipal principal,
            RecepcionDbContext db) =>
        {
            var employeeId = principal.GetEmployeeId();
            if (employeeId is null)
            {
                return Results.BadRequest(new[] { "El usuario no está vinculado a un empleado. Contacte al administrador para habilitar el acceso móvil." });
            }

            var webOrder = await db.WebOrders.FirstOrDefaultAsync(x => x.Id == id && x.EmployeeId == employeeId.Value);
            if (webOrder is null)
            {
                return Results.NotFound();
            }

            if (webOrder.Status != WebOrderStatus.Programado)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Solo se pueden marcar como recogidos los pedidos programados.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            webOrder.Status = WebOrderStatus.Recogido;
            webOrder.PickupObservation = request.Observation;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Pedido marcado como recogido.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("CompletePickupMobileWebOrder")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

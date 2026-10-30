using Microsoft.EntityFrameworkCore;

public static class ScheduleWebOrderEndpoint
{
    public static void MapScheduleWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:long}/schedule", async (long id, ScheduleWebOrderRequest request, RecepcionDbContext db) =>
        {
            var webOrder = await db.WebOrders.FirstOrDefaultAsync(x => x.Id == id);
            if (webOrder is null)
            {
                return Results.NotFound();
            }

            if (webOrder.Status != WebOrderStatus.Confirmado)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Solo se puede programar la logística de pedidos ya confirmados.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var vehicleExists = await db.Vehicles.AnyAsync(x => x.Id == request.VehicleId && x.IsActive);
            if (!vehicleExists)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El vehículo indicado no existe o está inactivo.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var employeeExists = await db.Employees.AnyAsync(x => x.Id == request.EmployeeId);
            if (!employeeExists)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El empleado indicado no existe.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            // El front manda un Date de JS, que se serializa como UTC. Si llega con Kind=Utc, se
            // convierte a hora Peru antes de guardar; si llega sin offset, ya se asume hora Peru.
            var scheduledPickupDate = request.ScheduledPickupDate.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(request.ScheduledPickupDate, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
                : DateTime.SpecifyKind(request.ScheduledPickupDate, DateTimeKind.Unspecified);

            webOrder.VehicleId = request.VehicleId;
            webOrder.EmployeeId = request.EmployeeId;
            webOrder.ScheduledPickupDate = scheduledPickupDate;
            webOrder.Status = WebOrderStatus.Programado;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Logística programada.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("ScheduleWebOrder")
        .WithTags("WebOrders")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionPedidosWeb);
    }
}

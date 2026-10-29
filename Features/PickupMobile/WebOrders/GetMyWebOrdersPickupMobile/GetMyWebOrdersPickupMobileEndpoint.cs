using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetMyWebOrdersPickupMobileEndpoint
{
    public static void MapGetMyWebOrdersPickupMobile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/my-web-orders", async (ClaimsPrincipal principal, WebOrderStatus? status, RecepcionDbContext db) =>
        {
            var employeeId = principal.GetEmployeeId();
            if (employeeId is null)
            {
                return Results.BadRequest(new[] { "El usuario no está vinculado a un empleado. Contacte al administrador para habilitar el acceso móvil." });
            }

            var query = db.WebOrders
                .AsNoTracking()
                .Include(x => x.WorkGuideMain)
                .Where(x => x.EmployeeId == employeeId.Value)
                .AsQueryable();

            query = status.HasValue
                ? query.Where(x => x.Status == status.Value)
                : query.Where(x => x.Status == WebOrderStatus.Programado);

            var orders = await query
                .OrderBy(x => x.ScheduledPickupDate)
                .Select(x => new PickupMobileWebOrderDto(
                    x.Id, x.OrderNumber, x.DeliveryAddress, x.ContactPhone, x.ScheduledPickupDate, x.Status,
                    x.WorkGuideMain != null ? x.WorkGuideMain.SerieGuia : null,
                    x.WorkGuideMain != null ? x.WorkGuideMain.NumeroGuia : null))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<PickupMobileWebOrderDto>>
            {
                Success = true,
                Message = "Mis pedidos web.",
                StatusCode = StatusCodes.Status200OK,
                Data = orders
            });
        })
        .WithName("GetMyWebOrdersPickupMobile")
        .Produces<ApiResponse<List<PickupMobileWebOrderDto>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

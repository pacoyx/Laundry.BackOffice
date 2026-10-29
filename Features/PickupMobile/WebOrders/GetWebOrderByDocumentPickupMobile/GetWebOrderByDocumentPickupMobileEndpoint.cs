using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetWebOrderByDocumentPickupMobileEndpoint
{
    public static void MapGetWebOrderByDocumentPickupMobile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/web-orders/document/{serie}/{numero}", async (string serie, string numero, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var employeeId = principal.GetEmployeeId();
            if (employeeId is null)
            {
                return Results.BadRequest(new[] { "El usuario no está vinculado a un empleado. Contacte al administrador para habilitar el acceso móvil." });
            }

            var webOrder = await db.WebOrders
                .AsNoTracking()
                .Include(x => x.WorkGuideMain)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Value
                    && x.WorkGuideMain != null
                    && x.WorkGuideMain.SerieGuia == serie
                    && x.WorkGuideMain.NumeroGuia == numero);

            if (webOrder is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se encontró un pedido asignado a ti con ese número de guía.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var response = new PickupMobileWebOrderDto(
                webOrder.Id, webOrder.OrderNumber, webOrder.DeliveryAddress, webOrder.ContactPhone,
                webOrder.ScheduledPickupDate, webOrder.Status,
                webOrder.WorkGuideMain!.SerieGuia, webOrder.WorkGuideMain!.NumeroGuia);

            return Results.Ok(new ApiResponse<PickupMobileWebOrderDto>
            {
                Success = true,
                Message = "Pedido encontrado.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetWebOrderByDocumentPickupMobile")
        .Produces<ApiResponse<PickupMobileWebOrderDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

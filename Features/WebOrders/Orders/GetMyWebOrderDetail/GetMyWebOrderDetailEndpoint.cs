using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetMyWebOrderDetailEndpoint
{
    public static void MapGetMyWebOrderDetail(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}", async (long id, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetWebCustomerCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var webOrder = await db.WebOrders
                .AsNoTracking()
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .Include(x => x.WorkGuideMain).ThenInclude(g => g!.WorkGuideDetails).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (webOrder is null)
            {
                return Results.NotFound();
            }

            var photos = new List<WebOrderEvidencePhotoResponse>();
            if (webOrder.WorkGuideMainId.HasValue)
            {
                photos = await db.ItemPhotos
                    .AsNoTracking()
                    .Where(x => x.WorkGuideMainId == webOrder.WorkGuideMainId.Value)
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new WebOrderEvidencePhotoResponse(x.Id, x.BlobUrl, x.Stage, x.CreatedAt))
                    .ToListAsync();
            }

            // Una vez generada la guia, el personal puede cambiar items, cantidades/pesos o precios
            // directamente en ella; se refleja esa version vigente en vez del snapshot original del pedido.
            var details = webOrder.WorkGuideMain is not null
                ? webOrder.WorkGuideMain.WorkGuideDetails
                    .Where(d => d.EstadoRegistro == "A")
                    .Select(d => new WebOrderDetailResponse(d.ProductId, d.Product!.Name, d.Cant, d.Precio, d.Total))
                    .ToList()
                : webOrder.Details.Select(d => new WebOrderDetailResponse(d.ServiceItemId, d.ServiceItem.Name, d.Quantity, d.UnitPrice, d.Total)).ToList();

            var total = webOrder.WorkGuideMain?.Total ?? webOrder.Total;

            var response = new WebOrderDetailFullResponse(
                webOrder.Id,
                webOrder.OrderNumber,
                webOrder.Status,
                webOrder.DeliveryAddress,
                webOrder.ContactPhone,
                webOrder.RequestedPickupDate,
                webOrder.ScheduledPickupDate,
                webOrder.AdditionalItemsNote,
                total,
                webOrder.CreatedAt,
                webOrder.WorkGuideMain?.SerieGuia,
                webOrder.WorkGuideMain?.NumeroGuia,
                details,
                photos);

            return Results.Ok(new ApiResponse<WebOrderDetailFullResponse>
            {
                Success = true,
                Message = "Detalle del pedido.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetMyWebOrderDetail")
        .WithTags("WebOrders")
        .Produces<ApiResponse<WebOrderDetailFullResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}

using Microsoft.EntityFrameworkCore;

public static class GetPickupMobileServiceItemsEndpoint
{
    public static void MapGetPickupMobileServiceItems(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customers/{customerId:int}/service-items", async (int customerId, RecepcionDbContext db) =>
        {
            // Catálogo de servicios con precio vigente para el cliente, expuesto al canal móvil
            // (el endpoint equivalente /api/customers/{id}/price-lists/active exige corporate-operations.access,
            // permiso que el personal de recojo no posee).
            var priceList = await db.CustomerPriceLists
                .AsNoTracking()
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.IsActive);

            // La tarifa siempre trae una fila por cada servicio del catalogo (ver
            // DialogFormTarifaComponent en el frontend: completa UnitPrice=0 para los que no
            // se definio precio), asi que sin este filtro el combo terminaba mostrando el
            // catalogo entero en vez de solo lo contratado para este cliente.
            var items = priceList?.Details
                .Where(d => d.UnitPrice > 0)
                .Select(d => new PickupMobileServiceItemLookupDto(d.ServiceItemId, d.ServiceItem.Name, d.UnitPrice))
                .OrderBy(x => x.Name)
                .ToList() ?? new List<PickupMobileServiceItemLookupDto>();

            return Results.Ok(new ApiResponse<List<PickupMobileServiceItemLookupDto>>
            {
                Success = true,
                Message = items.Count > 0 ? "Servicios disponibles para el cliente." : "El cliente no tiene una tarifa activa.",
                StatusCode = StatusCodes.Status200OK,
                Data = items
            });
        })
        .WithName("GetPickupMobileServiceItems")
        .Produces<ApiResponse<List<PickupMobileServiceItemLookupDto>>>(StatusCodes.Status200OK);
    }
}

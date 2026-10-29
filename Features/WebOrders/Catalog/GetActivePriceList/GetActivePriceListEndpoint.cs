public static class GetActiveZonePriceListEndpoint
{
    public static void MapGetActiveZonePriceList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/price-list", async (RecepcionDbContext db, int districtId) =>
        {
            var (district, priceList) = await ZonePricingResolver.GetActivePriceListForDistrictAsync(db, districtId);

            if (district is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El distrito indicado no existe.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            if (priceList is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No hay una lista de precios activa para esa zona.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var response = new ActiveZonePriceListResponse(
                priceList.Id,
                district.ZoneId,
                district.Zone.Name,
                priceList.Details
                    .Select(d => new ZonePriceListItemResponse(d.ServiceItemId, d.ServiceItem.Name, d.ServiceItem.Description, d.UnitPrice))
                    .ToList());

            return Results.Ok(new ApiResponse<ActiveZonePriceListResponse>
            {
                Success = true,
                Message = "Lista de precios de la zona.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetWebOrderActivePriceList")
        .WithTags("WebOrders")
        .Produces<ApiResponse<ActiveZonePriceListResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}

using Microsoft.EntityFrameworkCore;

public static class GetZonePriceListEndpoint
{
    public static void MapGetZonePriceList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/zone-price-lists/{id:long}", async (long id, RecepcionDbContext db) =>
        {
            var priceList = await db.ZonePriceLists
                .AsNoTracking()
                .Include(x => x.Zone)
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (priceList is null)
            {
                return Results.NotFound("Tarifa no encontrada.");
            }

            return Results.Ok(new ApiResponse<ZonePriceListAdminResponse>
            {
                Success = true,
                Message = "Tarifa de zona.",
                StatusCode = StatusCodes.Status200OK,
                Data = ZonePriceListsAdminMapper.ToResponse(priceList)
            });
        })
        .WithName("GetZonePriceList")
        .Produces<ApiResponse<ZonePriceListAdminResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}

using Microsoft.EntityFrameworkCore;

public static class GetZonePriceListsEndpoint
{
    public static void MapGetZonePriceLists(this IEndpointRouteBuilder app)
    {
        app.MapGet("/zones/{zoneId:int}/price-lists", async (int zoneId, RecepcionDbContext db) =>
        {
            var priceLists = await db.ZonePriceLists
                .AsNoTracking()
                .Include(x => x.Zone)
                .Where(x => x.ZoneId == zoneId)
                .OrderByDescending(x => x.Version)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<ZonePriceListAdminSummaryResponse>>
            {
                Success = true,
                Message = "Listado de tarifas de la zona.",
                StatusCode = StatusCodes.Status200OK,
                Data = priceLists.Select(ZonePriceListsAdminMapper.ToSummaryResponse).ToList()
            });
        })
        .WithName("GetZonePriceLists")
        .Produces<ApiResponse<List<ZonePriceListAdminSummaryResponse>>>(StatusCodes.Status200OK)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}

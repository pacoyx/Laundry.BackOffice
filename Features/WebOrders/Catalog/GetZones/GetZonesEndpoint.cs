using Microsoft.EntityFrameworkCore;

public static class GetZonesEndpoint
{
    public static void MapGetZones(this IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/zones", async (RecepcionDbContext db) =>
        {
            var zones = await db.Zones
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new ZoneResponse(x.Id, x.Name))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<ZoneResponse>>
            {
                Success = true,
                Message = "Zonas disponibles.",
                StatusCode = StatusCodes.Status200OK,
                Data = zones
            });
        })
        .WithName("GetWebOrderZones")
        .WithTags("WebOrders")
        .Produces<ApiResponse<List<ZoneResponse>>>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}

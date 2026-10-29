using Microsoft.EntityFrameworkCore;

public static class GetDistrictsEndpoint
{
    public static void MapGetDistricts(this IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/districts", async (RecepcionDbContext db, int? zoneId) =>
        {
            var query = db.Districts
                .Include(x => x.Zone)
                .Where(x => x.IsActive)
                .AsQueryable();

            if (zoneId.HasValue)
            {
                query = query.Where(x => x.ZoneId == zoneId.Value);
            }

            var districts = await query
                .OrderBy(x => x.Name)
                .Select(x => new DistrictResponse(x.Id, x.Name, x.ZoneId, x.Zone.Name))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<DistrictResponse>>
            {
                Success = true,
                Message = "Distritos disponibles.",
                StatusCode = StatusCodes.Status200OK,
                Data = districts
            });
        })
        .WithName("GetWebOrderDistricts")
        .WithTags("WebOrders")
        .Produces<ApiResponse<List<DistrictResponse>>>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}

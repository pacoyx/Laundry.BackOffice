using Microsoft.EntityFrameworkCore;

public static class GetDistrictsAdminEndpoint
{
    public static void MapGetDistrictsAdmin(this IEndpointRouteBuilder app)
    {
        app.MapGet("/districts", async (RecepcionDbContext db) =>
        {
            var districts = await db.Districts
                .AsNoTracking()
                .Include(x => x.Zone)
                .OrderBy(x => x.Name)
                .Select(x => new DistrictAdminResponse(x.Id, x.Name, x.ZoneId, x.Zone.Name, x.IsActive))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<DistrictAdminResponse>>
            {
                Success = true,
                Message = "Distritos.",
                StatusCode = StatusCodes.Status200OK,
                Data = districts
            });
        })
        .WithName("GetDistrictsAdmin")
        .Produces<ApiResponse<List<DistrictAdminResponse>>>(StatusCodes.Status200OK)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}

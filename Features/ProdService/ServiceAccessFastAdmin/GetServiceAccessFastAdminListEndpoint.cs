using Microsoft.EntityFrameworkCore;

public static class GetServiceAccessFastAdminListEndpoint
{
    public static void MapGetServiceAccessFastAdminList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/access-fast", async (RecepcionDbContext db) =>
        {
            var items = await db.ServiceAccessFasts
                .AsNoTracking()
                .Join(db.ProdServices, saf => saf.ProdServiceID, ps => ps.Id, (saf, ps) => new ServiceAccessFastAdminDto(
                    saf.Id,
                    saf.ProdServiceID,
                    ps.Name,
                    saf.ShortName,
                    saf.IconName,
                    saf.Status,
                    saf.Order
                ))
                .OrderBy(x => x.Order)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<ServiceAccessFastAdminDto>>
            {
                Success = true,
                Message = "Listado de accesos rápidos.",
                StatusCode = StatusCodes.Status200OK,
                Data = items
            });
        })
        .WithName("GetServiceAccessFastAdminList")
        .Produces<ApiResponse<List<ServiceAccessFastAdminDto>>>(StatusCodes.Status200OK);
    }
}

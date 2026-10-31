using Microsoft.EntityFrameworkCore;

public static class GetFixedAssetsEndpoint
{
    public static void MapGetFixedAssets(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            int? branchId,
            int? categoryId,
            FixedAssetStatus? status,
            RecepcionDbContext db) =>
        {
            var query = db.FixedAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Branch)
                .Include(x => x.ResponsibleUser)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var assets = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<FixedAssetListItemResponse>>
            {
                Success = true,
                Message = "Listado de activos fijos.",
                StatusCode = StatusCodes.Status200OK,
                Data = assets.Select(FixedAssetsEndpoints.ToListItemResponse).ToList()
            });
        })
        .WithName("GetFixedAssets")
        .Produces<ApiResponse<List<FixedAssetListItemResponse>>>(StatusCodes.Status200OK);
    }
}

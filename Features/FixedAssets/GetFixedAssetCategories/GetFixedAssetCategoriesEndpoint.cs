using Microsoft.EntityFrameworkCore;

public static class GetFixedAssetCategoriesEndpoint
{
    public static void MapGetFixedAssetCategories(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var categories = await db.FixedAssetCategories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<FixedAssetCategoryResponse>>
            {
                Success = true,
                Message = "Categorías de activo fijo.",
                StatusCode = StatusCodes.Status200OK,
                Data = categories.Select(c => new FixedAssetCategoryResponse(
                    c.Id, c.Name, c.DefaultDepreciationRatePercent, c.DefaultUsefulLifeYears
                )).ToList()
            });
        })
        .WithName("GetFixedAssetCategories")
        .Produces<ApiResponse<List<FixedAssetCategoryResponse>>>(StatusCodes.Status200OK);
    }
}

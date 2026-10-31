using Microsoft.EntityFrameworkCore;

public static class GetFixedAssetDetailEndpoint
{
    public static void MapGetFixedAssetDetail(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}", async (long id, RecepcionDbContext db) =>
        {
            var asset = await db.FixedAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Branch)
                .Include(x => x.ResponsibleUser)
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseInvoice)
                .Include(x => x.Maintenances).ThenInclude(m => m.Attachments)
                .Include(x => x.Reassignments).ThenInclude(r => r.PreviousBranch)
                .Include(x => x.Reassignments).ThenInclude(r => r.NewBranch)
                .Include(x => x.Reassignments).ThenInclude(r => r.PreviousResponsibleUser)
                .Include(x => x.Reassignments).ThenInclude(r => r.NewResponsibleUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            return Results.Ok(new ApiResponse<FixedAssetDetailResponse>
            {
                Success = true,
                Message = "Activo fijo obtenido correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = FixedAssetsEndpoints.ToDetailResponse(asset)
            });
        })
        .WithName("GetFixedAssetDetail")
        .Produces<ApiResponse<FixedAssetDetailResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

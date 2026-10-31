using Microsoft.EntityFrameworkCore;

public static class ReassignFixedAssetEndpoint
{
    public static void MapReassignFixedAsset(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/reassign", async (
            long id,
            ReassignFixedAssetRequest request,
            RecepcionDbContext db) =>
        {
            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            if (!await db.BranchSales.AnyAsync(x => x.Id == request.NewBranchId))
            {
                return Results.BadRequest(new[] { "La sucursal indicada no existe." });
            }

            if (request.NewResponsibleUserId is not null && !await db.Users.AnyAsync(x => x.Id == request.NewResponsibleUserId))
            {
                return Results.BadRequest(new[] { "El usuario responsable indicado no existe." });
            }

            var reassignment = new FixedAssetReassignment
            {
                FixedAssetId = asset.Id,
                Date = DateTime.UtcNow,
                PreviousBranchId = asset.BranchId,
                NewBranchId = request.NewBranchId,
                PreviousResponsibleUserId = asset.ResponsibleUserId,
                NewResponsibleUserId = request.NewResponsibleUserId,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            asset.BranchId = request.NewBranchId;
            asset.ResponsibleUserId = request.NewResponsibleUserId;

            await db.FixedAssetReassignments.AddAsync(reassignment);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Activo fijo reasignado correctamente.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("ReassignFixedAsset")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

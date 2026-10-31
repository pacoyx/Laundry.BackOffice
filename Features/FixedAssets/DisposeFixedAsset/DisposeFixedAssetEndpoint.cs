using Microsoft.EntityFrameworkCore;

public static class DisposeFixedAssetEndpoint
{
    private static readonly HashSet<FixedAssetStatus> AllowedDisposalStatuses = new()
    {
        FixedAssetStatus.Disposed,
        FixedAssetStatus.Sold
    };

    public static void MapDisposeFixedAsset(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/dispose", async (
            long id,
            DisposeFixedAssetRequest request,
            RecepcionDbContext db) =>
        {
            if (!AllowedDisposalStatuses.Contains(request.Status))
            {
                return Results.BadRequest(new[] { "El estado de baja debe ser 'Disposed' o 'Sold'." });
            }

            if (string.IsNullOrWhiteSpace(request.DisposalReason))
            {
                return Results.BadRequest(new[] { "Debes indicar el motivo de la baja." });
            }

            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            asset.Status = request.Status;
            asset.DisposalDate = request.DisposalDate;
            asset.DisposalReason = request.DisposalReason;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Activo fijo dado de baja correctamente.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("DisposeFixedAsset")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

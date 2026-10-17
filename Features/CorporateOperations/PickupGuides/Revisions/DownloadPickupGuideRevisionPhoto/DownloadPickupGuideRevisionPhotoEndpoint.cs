using Microsoft.EntityFrameworkCore;

public static class DownloadPickupGuideRevisionPhotoEndpoint
{
    public static void MapDownloadPickupGuideRevisionPhoto(this IEndpointRouteBuilder app)
    {
        app.MapGet("/revisions/photos/{photoId:long}/download", async (
            long photoId,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var photo = await db.PickupGuideRevisionPhotos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound("Fotografía no encontrada.");
            }

            var stream = await storage.OpenReadAsync(photo.FilePath, cancellationToken);

            return Results.File(stream, photo.ContentType, enableRangeProcessing: true);
        })
        .WithName("DownloadPickupGuideRevisionPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

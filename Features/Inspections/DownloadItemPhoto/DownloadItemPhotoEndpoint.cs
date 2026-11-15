using Microsoft.EntityFrameworkCore;

public static class DownloadItemPhotoEndpoint
{
    public static void MapDownloadItemPhoto(this IEndpointRouteBuilder app)
    {
        // GET /photos/{id}/download            -> foto completa
        // GET /photos/{id}/download?thumbnail=true -> miniatura (~320 px) para listados
        app.MapGet("/photos/{photoId:long}/download", async (
            long photoId,
            bool? thumbnail,
            RecepcionDbContext db,
            IFileStorageService storage,
            IImageCompressor compressor,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var photo = await db.ItemPhotos.FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound("Fotografía no encontrada.");
            }

            if (thumbnail == true)
            {
                if (photo.ThumbnailUrl is not null)
                {
                    var thumbStream = await storage.OpenReadAsync(photo.ThumbnailUrl, cancellationToken);
                    return Results.File(thumbStream, "image/jpeg", enableRangeProcessing: true);
                }

                // Foto anterior a las miniaturas: se genera ahora y queda guardada para las siguientes veces.
                byte[] original;
                await using (var source = await storage.OpenReadAsync(photo.BlobUrl, cancellationToken))
                using (var buffer = new MemoryStream())
                {
                    await source.CopyToAsync(buffer, cancellationToken);
                    original = buffer.ToArray();
                }

                try
                {
                    var created = await ItemPhotoThumbnails.CreateAndStoreAsync(photo, original, compressor, storage, cancellationToken);
                    if (created is not null)
                    {
                        photo.ThumbnailUrl = created.Value.Path;
                        await db.SaveChangesAsync(cancellationToken);
                        return Results.File(created.Value.Content, "image/jpeg");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    loggerFactory.CreateLogger("ItemPhotoThumbnails")
                        .LogWarning(ex, "No se pudo generar la miniatura de la foto {PhotoId}", photoId);
                }

                // Si no se pudo generar, se sirve la foto completa para no dejar la miniatura en blanco.
                return Results.File(original, photo.ContentType);
            }

            var stream = await storage.OpenReadAsync(photo.BlobUrl, cancellationToken);

            return Results.File(stream, photo.ContentType, enableRangeProcessing: true);
        })
        .WithName("DownloadItemPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

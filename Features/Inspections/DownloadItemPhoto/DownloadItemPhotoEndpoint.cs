using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

public static class DownloadItemPhotoEndpoint
{
    /// <summary>
    /// Las fotos de guía nunca cambian (una foto nueva es otro registro con otro ID), así que se pueden
    /// guardar en la caché del navegador indefinidamente. "private": solo la caché del navegador del
    /// usuario, nunca un proxy compartido (las fotos requieren sesión).
    /// </summary>
    private const string ImmutableCacheControl = "private, max-age=31536000, immutable";

    public static void MapDownloadItemPhoto(this IEndpointRouteBuilder app)
    {
        // GET /photos/{id}/download                -> foto completa
        // GET /photos/{id}/download?thumbnail=true -> miniatura (~320 px) para listados
        app.MapGet("/photos/{photoId:long}/download", async (
            long photoId,
            bool? thumbnail,
            HttpContext httpContext,
            RecepcionDbContext db,
            IFileStorageService storage,
            IImageCompressor compressor,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var wantsThumbnail = thumbnail == true;
            var etag = new EntityTagHeaderValue($"\"photo-{photoId}-{(wantsThumbnail ? "thumb" : "full")}\"");

            // Revalidación (p. ej. al recargar la página): si el navegador ya tiene esta versión, se responde
            // 304 sin consultar la base ni descargar nada del almacenamiento.
            if (MatchesETag(httpContext.Request, etag))
            {
                httpContext.Response.Headers.CacheControl = ImmutableCacheControl;
                httpContext.Response.Headers.ETag = etag.ToString();
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            var photo = await db.ItemPhotos.FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound("Fotografía no encontrada.");
            }

            var lastModified = new DateTimeOffset(DateTime.SpecifyKind(photo.CreatedAt, DateTimeKind.Utc));

            if (wantsThumbnail)
            {
                if (photo.ThumbnailUrl is not null)
                {
                    var thumbStream = await storage.OpenReadAsync(photo.ThumbnailUrl, cancellationToken);
                    httpContext.Response.Headers.CacheControl = ImmutableCacheControl;
                    return Results.File(thumbStream, "image/jpeg", lastModified: lastModified, entityTag: etag, enableRangeProcessing: true);
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
                        httpContext.Response.Headers.CacheControl = ImmutableCacheControl;
                        return Results.File(created.Value.Content, "image/jpeg", lastModified: lastModified, entityTag: etag);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    loggerFactory.CreateLogger("ItemPhotoThumbnails")
                        .LogWarning(ex, "No se pudo generar la miniatura de la foto {PhotoId}", photoId);
                }

                // Si no se pudo generar, se sirve la foto completa para no dejar la miniatura en blanco, pero
                // sin caché: así el próximo pedido vuelve a intentar generar la miniatura real.
                httpContext.Response.Headers.CacheControl = "no-store";
                return Results.File(original, photo.ContentType);
            }

            var stream = await storage.OpenReadAsync(photo.BlobUrl, cancellationToken);
            httpContext.Response.Headers.CacheControl = ImmutableCacheControl;
            return Results.File(stream, photo.ContentType, lastModified: lastModified, entityTag: etag, enableRangeProcessing: true);
        })
        .WithName("DownloadItemPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status304NotModified)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static bool MatchesETag(HttpRequest request, EntityTagHeaderValue etag)
    {
        var ifNoneMatch = request.GetTypedHeaders().IfNoneMatch;
        return ifNoneMatch is { Count: > 0 }
            && ifNoneMatch.Any(x => x.Equals(EntityTagHeaderValue.Any) || x.Compare(etag, useStrongComparison: false));
    }
}

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Foto del activo fijo (una por activo). Se guarda en el IFileStorageService configurado
/// (Azure Blob en produccion, disco local en desarrollo si FileStorage:Provider=Local).
/// La imagen se re-comprime a JPEG para no guardar fotos de celular de varios MB.
/// </summary>
public static class FixedAssetPhotoEndpoints
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int PhotoMaxWidth = 1280;
    private const int PhotoJpegQuality = 80;

    public static void MapFixedAssetPhoto(this IEndpointRouteBuilder app)
    {
        // Sube o reemplaza la foto
        app.MapPost("/{id:long}/photo", async (
            long id,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage,
            IImageCompressor compressor,
            CancellationToken cancellationToken) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new[] { "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG y WEBP." });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(new[] { "La foto supera el tamaño máximo permitido de 10 MB." });
            }

            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            byte[] original;
            await using (var input = file.OpenReadStream())
            using (var buffer = new MemoryStream())
            {
                await input.CopyToAsync(buffer, cancellationToken);
                original = buffer.ToArray();
            }

            // Si los bytes no son una imagen, SkiaSharp lanza ArgumentException o el compresor
            // devuelve el mismo arreglo sin tocar; en ambos casos se rechaza con un mensaje claro.
            byte[] compressed;
            try
            {
                compressed = compressor.CompressToJpeg(original, PhotoMaxWidth, PhotoJpegQuality);
            }
            catch (ArgumentException)
            {
                compressed = original;
            }

            if (ReferenceEquals(compressed, original))
            {
                return Results.BadRequest(new[] { "El archivo no es una imagen válida." });
            }

            var previousPath = asset.PhotoPath;

            using var content = new MemoryStream(compressed);
            var upload = await storage.UploadAsync("FixedAssets", "Photo", id, $"activo-{id}.jpg", "image/jpeg", content, cancellationToken);

            asset.PhotoPath = upload.FilePath;
            asset.PhotoUpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(previousPath))
            {
                await storage.DeleteAsync(previousPath, cancellationToken);
            }

            return Results.Ok(new ApiResponse<FixedAssetPhotoResponse>
            {
                Success = true,
                Message = "Foto del activo actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new FixedAssetPhotoResponse(asset.Id, asset.PhotoUpdatedAt.Value)
            });
        })
        .DisableAntiforgery()
        .WithName("UploadFixedAssetPhoto")
        .Produces<ApiResponse<FixedAssetPhotoResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // Descarga la foto
        app.MapGet("/{id:long}/photo", async (
            long id,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var photoPath = await db.FixedAssets
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.PhotoPath)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(photoPath))
            {
                return Results.NotFound("El activo no tiene foto.");
            }

            var stream = await storage.OpenReadAsync(photoPath, cancellationToken);
            return Results.File(stream, "image/jpeg", enableRangeProcessing: true);
        })
        .WithName("GetFixedAssetPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Elimina la foto
        app.MapDelete("/{id:long}/photo", async (
            long id,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            if (!string.IsNullOrEmpty(asset.PhotoPath))
            {
                var path = asset.PhotoPath;
                asset.PhotoPath = null;
                asset.PhotoUpdatedAt = null;
                await db.SaveChangesAsync(cancellationToken);
                await storage.DeleteAsync(path, cancellationToken);
            }

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Foto eliminada.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("DeleteFixedAssetPhoto")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

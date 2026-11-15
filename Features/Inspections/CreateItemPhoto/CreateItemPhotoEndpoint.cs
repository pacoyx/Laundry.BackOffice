using Microsoft.EntityFrameworkCore;

public static class CreateItemPhotoEndpoint
{
    public static void MapCreateItemPhoto(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/photos", async (
            int id,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage,
            IImageCompressor compressor,
            ILoggerFactory loggerFactory) =>
        {
            return await ExecuteAsync(id, file, db, storage, compressor, loggerFactory, WorkGuideEvidenceStage.Revision, null);
        })
        .DisableAntiforgery()
        .WithName("CreateItemPhoto")
        .Produces<ApiResponse<ItemPhotoResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }

    public static async Task<IResult> ExecuteAsync(
        int workGuideMainId,
        IFormFile file,
        RecepcionDbContext db,
        IFileStorageService storage,
        IImageCompressor compressor,
        ILoggerFactory loggerFactory,
        WorkGuideEvidenceStage stage,
        long? employeeId)
    {
        var workGuideExists = await db.WorkGuideMains.AnyAsync(x => x.Id == workGuideMainId);
        if (!workGuideExists)
            return Results.NotFound("Guía de servicio no encontrada.");

        if (file.Length == 0)
            return Results.BadRequest(new[] { "El archivo está vacío." });

        // Se lee una sola vez en memoria (las fotos ya llegan comprimidas desde el navegador) para subir
        // el original y generar la miniatura sin volver a leer el archivo.
        byte[] content;
        await using (var input = file.OpenReadStream())
        using (var buffer = new MemoryStream())
        {
            await input.CopyToAsync(buffer);
            content = buffer.ToArray();
        }

        StorageFileResult uploadResult;
        using (var originalStream = new MemoryStream(content))
        {
            uploadResult = await storage.UploadAsync("Inspections", "Photos", workGuideMainId, file.FileName, file.ContentType, originalStream);
        }

        var photo = new ItemPhoto
        {
            WorkGuideMainId = workGuideMainId,
            FileName = uploadResult.StoredFileName,
            OriginalFileName = file.FileName,
            BlobUrl = uploadResult.FilePath,
            ContentType = file.ContentType,
            FileSize = uploadResult.FileSize,
            Stage = stage,
            EmployeeId = employeeId,
            CreatedAt = DateTime.UtcNow
        };

        // La miniatura es una optimización: si falla, la foto se guarda igual y la miniatura se
        // generará la primera vez que se pida (ver DownloadItemPhotoEndpoint).
        try
        {
            var thumbnail = await ItemPhotoThumbnails.CreateAndStoreAsync(photo, content, compressor, storage);
            photo.ThumbnailUrl = thumbnail?.Path;
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("ItemPhotoThumbnails")
                .LogWarning(ex, "No se pudo generar la miniatura de la foto de la guía {WorkGuideMainId}", workGuideMainId);
        }

        await db.ItemPhotos.AddAsync(photo);
        await db.SaveChangesAsync();

        var response = new ItemPhotoResponse(
            photo.Id, photo.WorkGuideMainId, photo.FileName, photo.OriginalFileName, photo.ContentType, photo.FileSize,
            photo.Stage, photo.EmployeeId, photo.CreatedAt);

        return Results.Created($"/api/workguides/{workGuideMainId}/photos", new ApiResponse<ItemPhotoResponse>
        {
            Success = true,
            Message = "Fotografía registrada correctamente.",
            StatusCode = StatusCodes.Status201Created,
            Data = response
        });
    }
}

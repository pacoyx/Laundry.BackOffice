using Microsoft.EntityFrameworkCore;

public static class CreateFixedAssetMaintenanceAttachmentEndpoint
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "application/pdf"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public static void MapCreateFixedAssetMaintenanceAttachment(this IEndpointRouteBuilder app)
    {
        app.MapPost("/maintenances/{maintenanceId:long}/attachments", async (
            long maintenanceId,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage) =>
        {
            var maintenance = await db.FixedAssetMaintenances.FirstOrDefaultAsync(x => x.Id == maintenanceId);
            if (maintenance is null)
            {
                return Results.NotFound("Mantenimiento no encontrado.");
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new[] { "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG, WEBP y PDF." });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(new[] { "El archivo supera el tamaño máximo permitido de 10 MB." });
            }

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("FixedAssets", "Maintenances", maintenanceId, file.FileName, file.ContentType, stream);

            var attachment = new FixedAssetMaintenanceAttachment
            {
                FixedAssetMaintenanceId = maintenanceId,
                OriginalFileName = file.FileName,
                StoredFileName = uploadResult.StoredFileName,
                FilePath = uploadResult.FilePath,
                ContentType = file.ContentType,
                FileSize = uploadResult.FileSize,
                CreatedAt = DateTime.UtcNow
            };

            await db.FixedAssetMaintenanceAttachments.AddAsync(attachment);
            await db.SaveChangesAsync();

            var response = new ApiResponse<FixedAssetMaintenanceAttachmentResponse>
            {
                Success = true,
                Message = "Adjunto registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = new FixedAssetMaintenanceAttachmentResponse(
                    attachment.Id, attachment.OriginalFileName, attachment.ContentType, attachment.FileSize, attachment.CreatedAt
                )
            };

            return Results.Created($"/api/fixed-assets/maintenances/{maintenanceId}/attachments/{attachment.Id}", response);
        })
        .DisableAntiforgery()
        .WithName("CreateFixedAssetMaintenanceAttachment")
        .Produces<ApiResponse<FixedAssetMaintenanceAttachmentResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

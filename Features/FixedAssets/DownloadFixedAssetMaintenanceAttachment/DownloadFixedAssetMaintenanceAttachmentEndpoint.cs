using Microsoft.EntityFrameworkCore;

public static class DownloadFixedAssetMaintenanceAttachmentEndpoint
{
    public static void MapDownloadFixedAssetMaintenanceAttachment(this IEndpointRouteBuilder app)
    {
        app.MapGet("/maintenances/attachments/{attachmentId:long}/download", async (
            long attachmentId,
            RecepcionDbContext db,
            IFileStorageService storage) =>
        {
            var attachment = await db.FixedAssetMaintenanceAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attachmentId);

            if (attachment is null)
            {
                return Results.NotFound("Adjunto no encontrado.");
            }

            var stream = await storage.OpenReadAsync(attachment.FilePath);
            return Results.File(stream, attachment.ContentType, attachment.OriginalFileName, enableRangeProcessing: true);
        })
        .WithName("DownloadFixedAssetMaintenanceAttachment")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

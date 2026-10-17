using Microsoft.EntityFrameworkCore;

public static class CreatePickupGuideRevisionPhotoEndpoint
{
    public static void MapCreatePickupGuideRevisionPhoto(this IEndpointRouteBuilder app)
    {
        app.MapPost("/revisions/{revisionId:long}/photos", async (
            long revisionId,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage) =>
        {
            var revision = await db.PickupGuideRevisions
                .FirstOrDefaultAsync(x => x.Id == revisionId);

            if (revision is null)
            {
                return Results.NotFound("Revisión no encontrada.");
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("PickupGuides", "Revisions", revision.PickupGuideId, file.FileName, file.ContentType, stream);

            var photo = new PickupGuideRevisionPhoto
            {
                PickupGuideRevisionId = revisionId,
                FileName = uploadResult.StoredFileName,
                OriginalFileName = file.FileName,
                FilePath = uploadResult.FilePath,
                ContentType = file.ContentType,
                FileSize = uploadResult.FileSize,
                CreatedAt = DateTime.UtcNow
            };

            await db.PickupGuideRevisionPhotos.AddAsync(photo);
            await db.SaveChangesAsync();

            var response = PickupGuideRevisionsEndpoints.ToPhotoResponse(photo);

            return Results.Created($"/api/pickup-guides/revisions/{revisionId}/photos", new ApiResponse<PickupGuideRevisionPhotoResponse>
            {
                Success = true,
                Message = "Evidencia fotográfica registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = response
            });
        })
        .DisableAntiforgery()
        .WithName("CreatePickupGuideRevisionPhoto")
        .Produces<ApiResponse<PickupGuideRevisionPhotoResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

using Microsoft.EntityFrameworkCore;

public static class GetPickupGuideRevisionPhotosEndpoint
{
    public static void MapGetPickupGuideRevisionPhotos(this IEndpointRouteBuilder app)
    {
        app.MapGet("/revisions/{revisionId:long}/photos", async (long revisionId, RecepcionDbContext db) =>
        {
            var revisionExists = await db.PickupGuideRevisions.AnyAsync(x => x.Id == revisionId);
            if (!revisionExists)
            {
                return Results.NotFound("Revisión no encontrada.");
            }

            var photos = await db.PickupGuideRevisionPhotos
                .AsNoTracking()
                .Where(x => x.PickupGuideRevisionId == revisionId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new PickupGuideRevisionPhotoResponse(
                    x.Id, x.PickupGuideRevisionId, x.FileName, x.OriginalFileName, x.ContentType, x.FileSize, x.CreatedAt))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<PickupGuideRevisionPhotoResponse>>
            {
                Success = true,
                Message = "Listado de evidencias fotográficas de la revisión.",
                StatusCode = StatusCodes.Status200OK,
                Data = photos
            });
        })
        .WithName("GetPickupGuideRevisionPhotos")
        .Produces<ApiResponse<List<PickupGuideRevisionPhotoResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

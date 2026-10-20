using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class DownloadPortalPickupGuideRevisionPhotoEndpoint
{
    public static void MapDownloadPortalPickupGuideRevisionPhoto(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-guides/revisions/photos/{photoId:long}/download", async (
            long photoId,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Filtrado obligatorio por CustomerId del token: nunca por el request.
            var photo = await db.PickupGuideRevisionPhotos
                .AsNoTracking()
                .Include(x => x.PickupGuideRevision).ThenInclude(r => r.PickupGuide)
                .FirstOrDefaultAsync(x => x.Id == photoId
                    && x.PickupGuideRevision.PickupGuide.CustomerId == customerId.Value, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound();
            }

            var stream = await storage.OpenReadAsync(photo.FilePath, cancellationToken);

            return Results.File(stream, photo.ContentType, enableRangeProcessing: true);
        })
        .WithName("DownloadPortalPickupGuideRevisionPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GuidesView);
    }
}

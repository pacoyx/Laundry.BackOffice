using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class DownloadMyWebOrderPhotoEndpoint
{
    public static void MapDownloadMyWebOrderPhoto(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{orderId:long}/photos/{photoId:long}/download", async (
            long orderId,
            long photoId,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var customerId = principal.GetWebCustomerCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var webOrder = await db.WebOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId.Value, cancellationToken);

            if (webOrder is null || webOrder.WorkGuideMainId is null)
            {
                return Results.NotFound("Fotografía no encontrada.");
            }

            var photo = await db.ItemPhotos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == photoId && x.WorkGuideMainId == webOrder.WorkGuideMainId.Value, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound("Fotografía no encontrada.");
            }

            var stream = await storage.OpenReadAsync(photo.BlobUrl, cancellationToken);

            return Results.File(stream, photo.ContentType, enableRangeProcessing: true);
        })
        .WithName("DownloadMyWebOrderPhoto")
        .WithTags("WebOrders")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}

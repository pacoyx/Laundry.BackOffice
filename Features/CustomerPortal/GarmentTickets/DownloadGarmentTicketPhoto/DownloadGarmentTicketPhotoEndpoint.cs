using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class DownloadGarmentTicketPhotoEndpoint
{
    public static void MapDownloadGarmentTicketPhoto(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garment-tickets/photos/{photoId:long}/download", async (
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
            var photo = await db.GarmentIntakeTicketPhotos
                .AsNoTracking()
                .Include(x => x.Ticket)
                .FirstOrDefaultAsync(x => x.Id == photoId && x.Ticket.CustomerId == customerId.Value, cancellationToken);

            if (photo is null)
            {
                return Results.NotFound();
            }

            var stream = await storage.OpenReadAsync(photo.FilePath, cancellationToken);

            return Results.File(stream, photo.ContentType, enableRangeProcessing: true);
        })
        .WithName("DownloadGarmentTicketPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

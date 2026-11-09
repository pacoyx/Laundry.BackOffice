using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class CreateGarmentTicketPhotoEndpoint
{
    public static void MapCreateGarmentTicketPhoto(this IEndpointRouteBuilder app)
    {
        app.MapPost("/garment-tickets/{id:int}/photos", async (
            int id,
            ClaimsPrincipal principal,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage) =>
        {
            var customerId = principal.GetCustomerId();
            var customerUserId = principal.GetCustomerUserId();
            if (customerId is null || customerUserId is null)
            {
                return Results.Unauthorized();
            }

            var ticket = await db.GarmentIntakeTickets
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (ticket is null)
            {
                return Results.NotFound();
            }

            // Una vez recogido (PickedUp) o anulado el ticket queda inmutable: no admite nuevas fotos.
            if (ticket.Status != GarmentIntakeTicketStatus.Pending)
            {
                return Results.BadRequest(new[] { "Solo se pueden adjuntar fotos a tickets pendientes." });
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("GarmentIntakeTickets", "Photos", ticket.Id, file.FileName, file.ContentType, stream);

            var photo = new GarmentIntakeTicketPhoto
            {
                TicketId = ticket.Id,
                FileName = uploadResult.StoredFileName,
                OriginalFileName = file.FileName,
                FilePath = uploadResult.FilePath,
                ContentType = file.ContentType,
                FileSize = uploadResult.FileSize,
                CreatedAt = DateTime.UtcNow,
                CreatedByCustomerUserId = customerUserId.Value
            };

            await db.GarmentIntakeTicketPhotos.AddAsync(photo);
            await db.SaveChangesAsync();

            var response = new ApiResponse<GarmentIntakeTicketPhotoResponse>
            {
                Success = true,
                Message = "Foto registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = GarmentTicketsEndpoints.ToPhotoResponse(photo)
            };

            return Results.Created($"/api/customer-portal/garment-tickets/{ticket.Id}/photos/{photo.Id}", response);
        })
        .DisableAntiforgery()
        .WithName("CreateGarmentTicketPhoto")
        .Produces<ApiResponse<GarmentIntakeTicketPhotoResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

using Microsoft.EntityFrameworkCore;

public static class DeleteSharedDocumentEndpoint
{
    public static void MapDeleteSharedDocument(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id:long}", async (
            long id,
            System.Security.Claims.ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var document = await db.SharedDocuments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (document is null)
            {
                return Results.NotFound("Documento no encontrado.");
            }

            if (document.UploadedByUserId != userId.Value)
            {
                return Results.Forbid();
            }

            await storage.DeleteAsync(document.BlobUrl, cancellationToken);

            db.SharedDocuments.Remove(document);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Documento eliminado correctamente.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .RequirePermission(PermissionCodes.SharedDocuments)
        .WithName("DeleteSharedDocument")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

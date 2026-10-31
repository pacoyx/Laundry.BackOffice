using Microsoft.EntityFrameworkCore;

public static class GetSharedDocumentFileEndpoint
{
    public static void MapGetSharedDocumentFile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}/file", async (
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

            var document = await db.SharedDocuments
                .AsNoTracking()
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (document is null)
            {
                return Results.NotFound("Documento no encontrado.");
            }

            // Solo quien lo subio o quien esta en la lista de destinatarios puede
            // descargarlo. Sin este chequeo, cualquier usuario autenticado podria
            // ver documentos de otros solo adivinando el Id.
            var tienePermiso = document.UploadedByUserId == userId.Value
                || document.Recipients.Any(r => r.UserId == userId.Value);

            if (!tienePermiso)
            {
                return Results.Forbid();
            }

            var stream = await storage.OpenReadAsync(document.BlobUrl, cancellationToken);
            return Results.File(stream, document.ContentType, document.FileName, enableRangeProcessing: true);
        })
        .WithName("GetSharedDocumentFile")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

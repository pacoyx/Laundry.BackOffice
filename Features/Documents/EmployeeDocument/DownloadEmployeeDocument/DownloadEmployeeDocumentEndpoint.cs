using Microsoft.EntityFrameworkCore;

public static class DownloadEmployeeDocumentEndpoint
{
    public static RouteGroupBuilder MapDownloadEmployeeDocument(this RouteGroupBuilder group)
    {
        group.MapGet("/{employeeId:long}/documents/{documentId:long}/download", async (
            long employeeId,
            long documentId,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var document = await db.EmployeeDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == documentId && x.EmployeeId == employeeId, cancellationToken);

            if (document is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Documento con ID {documentId} no encontrado para el trabajador {employeeId}."
                });
            }

            var stream = await storage.OpenReadAsync(document.BlobUrl, cancellationToken);

            return Results.File(stream, document.ContentType, document.FileName, enableRangeProcessing: true);
        })
        .WithName("DownloadEmployeeDocument")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}

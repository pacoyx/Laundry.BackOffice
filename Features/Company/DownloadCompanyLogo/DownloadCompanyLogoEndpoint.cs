using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

public static class DownloadCompanyLogoEndpoint
{
    public static void MapDownloadCompanyLogo(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}/logo", async (
            int id,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (company is null || string.IsNullOrEmpty(company.Logo))
            {
                return Results.NotFound();
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(company.Logo, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var stream = await storage.OpenReadAsync(company.Logo, cancellationToken);
            return Results.File(stream, contentType, enableRangeProcessing: true);
        })
        .WithName("DownloadCompanyLogo")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

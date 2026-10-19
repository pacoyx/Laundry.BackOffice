using Microsoft.EntityFrameworkCore;

public static class UploadCompanyLogoEndpoint
{
    private static readonly HashSet<string> AllowedLogoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxLogoSizeBytes = 2 * 1024 * 1024;

    public static void MapUploadCompanyLogo(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/logo", async (
            int id,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!AllowedLogoContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new[] { "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG y WEBP." });
            }

            if (file.Length > MaxLogoSizeBytes)
            {
                return Results.BadRequest(new[] { "El archivo supera el tamaño máximo permitido de 2 MB." });
            }

            var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (company is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Empresa con ID {id} no encontrada."
                });
            }

            var previousLogo = company.Logo;

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("Companies", "Logo", id, file.FileName, file.ContentType, stream, cancellationToken);

            company.Logo = uploadResult.FilePath;
            await db.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(previousLogo))
            {
                await storage.DeleteAsync(previousLogo, cancellationToken);
            }

            var response = new ApiResponse<CompanyLogoResponseDto>
            {
                Success = true,
                Message = "Logo actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new CompanyLogoResponseDto(company.Id, company.Logo)
            };

            return Results.Ok(response);
        })
        .DisableAntiforgery()
        .WithName("UploadCompanyLogo")
        .Produces<ApiResponse<CompanyLogoResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

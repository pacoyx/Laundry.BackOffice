using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public static class CreateSharedDocumentEndpoint
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public static void MapCreateSharedDocument(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            [FromForm] string title,
            [FromForm] string category,
            [FromForm] string? notes,
            [FromForm] int[] recipientUserIds,
            IFormFile file,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var uploadedByUserId = principal.GetUserId();
            if (uploadedByUserId is null)
            {
                return Results.Unauthorized();
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new[] { "Solo se permiten archivos PDF." });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(new[] { "El archivo supera el tamaño máximo permitido de 10 MB." });
            }

            if (recipientUserIds is null || recipientUserIds.Length == 0)
            {
                return Results.BadRequest(new[] { "Debes seleccionar al menos un destinatario." });
            }

            var recipientIds = recipientUserIds.Distinct().ToList();
            var existingUserIds = await db.Users
                .Where(x => recipientIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var missing = recipientIds.Except(existingUserIds).ToList();
            if (missing.Count > 0)
            {
                return Results.BadRequest(new[] { $"Los siguientes usuarios no existen: {string.Join(", ", missing)}" });
            }

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync(
                "SharedDocuments",
                Slugify(category),
                uploadedByUserId.Value,
                file.FileName,
                file.ContentType,
                stream,
                cancellationToken);

            var document = new SharedDocument
            {
                Title = title,
                Category = category,
                Notes = notes,
                FileName = file.FileName,
                BlobUrl = uploadResult.FilePath,
                ContentType = file.ContentType,
                UploadedByUserId = uploadedByUserId.Value,
                CreatedAt = DateTime.UtcNow,
                Recipients = recipientIds.Select(userId => new SharedDocumentRecipient { UserId = userId }).ToList()
            };

            await db.SharedDocuments.AddAsync(document, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var saved = await db.SharedDocuments
                .AsNoTracking()
                .Include(x => x.UploadedByUser)
                .Include(x => x.Recipients).ThenInclude(r => r.User)
                .FirstAsync(x => x.Id == document.Id, cancellationToken);

            var response = new ApiResponse<SharedDocumentResponse>
            {
                Success = true,
                Message = "Documento compartido correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = SharedDocumentsEndpoints.ToResponse(saved)
            };

            return Results.Created($"/api/shared-documents/{document.Id}", response);
        })
        .RequirePermission(PermissionCodes.SharedDocuments)
        .DisableAntiforgery()
        .WithName("CreateSharedDocument")
        .Produces<ApiResponse<SharedDocumentResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder();

        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        var slug = System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "-+", "-").Trim('-');
        return slug.Length == 0 ? "otros" : slug;
    }
}

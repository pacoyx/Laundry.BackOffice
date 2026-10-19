using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class CreateEmployeeDocumentService : ICreateEmployeeDocumentService
{
    private readonly RecepcionDbContext _context;
    private readonly IFileStorageService _storage;

    public CreateEmployeeDocumentService(RecepcionDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<CreateEmployeeDocumentResult> CreateEmployeeDocumentAsync(
        long employeeId,
        int documentTypeId,
        string? notes,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var employeeExists = await _context.Employees.AnyAsync(x => x.Id == employeeId, cancellationToken);
        if (!employeeExists)
        {
            return CreateEmployeeDocumentResult.NotFoundEmployee();
        }

        var documentType = await _context.EmployeeDocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentTypeId, cancellationToken);
        if (documentType is null)
        {
            return CreateEmployeeDocumentResult.NotFoundDocumentType();
        }

        await using var stream = file.OpenReadStream();
        var uploadResult = await _storage.UploadAsync(
            "Employees",
            Slugify(documentType.Name),
            employeeId,
            file.FileName,
            file.ContentType,
            stream,
            cancellationToken);

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentTypeId = documentTypeId,
            FileName = file.FileName,
            BlobUrl = uploadResult.FilePath,
            ContentType = file.ContentType,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateEmployeeDocumentResult.Ok(new EmployeeDocumentResponseDto
        {
            Id = document.Id,
            EmployeeId = document.EmployeeId,
            DocumentTypeId = document.DocumentTypeId,
            DocumentTypeName = documentType.Name,
            FileName = document.FileName,
            BlobUrl = document.BlobUrl,
            ContentType = document.ContentType,
            Notes = document.Notes,
            CreatedAt = document.CreatedAt
        });
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        var slug = System.Text.RegularExpressions.Regex.Replace(builder.ToString(), "-+", "-").Trim('-');
        return slug.Length == 0 ? "otros" : slug;
    }
}

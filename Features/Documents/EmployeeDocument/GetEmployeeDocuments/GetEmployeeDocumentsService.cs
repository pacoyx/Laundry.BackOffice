using Microsoft.EntityFrameworkCore;

public class GetEmployeeDocumentsService : IGetEmployeeDocumentsService
{
    private readonly RecepcionDbContext _context;

    public GetEmployeeDocumentsService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDocumentResponseDto>> GetEmployeeDocumentsAsync(long employeeId)
    {
        return await _context.EmployeeDocuments
            .AsNoTracking()
            .Include(x => x.DocumentType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EmployeeDocumentResponseDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeName = x.DocumentType!.Name,
                FileName = x.FileName,
                BlobUrl = x.BlobUrl,
                ContentType = x.ContentType,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }
}

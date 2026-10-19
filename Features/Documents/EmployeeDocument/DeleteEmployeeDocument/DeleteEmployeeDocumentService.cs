using Microsoft.EntityFrameworkCore;

public class DeleteEmployeeDocumentService : IDeleteEmployeeDocumentService
{
    private readonly RecepcionDbContext _context;
    private readonly IFileStorageService _storage;

    public DeleteEmployeeDocumentService(RecepcionDbContext context, IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<bool> DeleteEmployeeDocumentAsync(long employeeId, long documentId)
    {
        var document = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(x => x.Id == documentId && x.EmployeeId == employeeId);

        if (document == null)
        {
            return false;
        }

        _context.EmployeeDocuments.Remove(document);
        await _context.SaveChangesAsync();

        await _storage.DeleteAsync(document.BlobUrl);

        return true;
    }
}

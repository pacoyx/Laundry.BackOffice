public interface ICreateEmployeeDocumentService
{
    Task<CreateEmployeeDocumentResult> CreateEmployeeDocumentAsync(
        long employeeId,
        int documentTypeId,
        string? notes,
        IFormFile file,
        CancellationToken cancellationToken = default);
}

public record EmployeeDocumentResponseDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum CreateEmployeeDocumentError
{
    None,
    EmployeeNotFound,
    DocumentTypeNotFound
}

public record CreateEmployeeDocumentResult
{
    public CreateEmployeeDocumentError Error { get; init; } = CreateEmployeeDocumentError.None;
    public EmployeeDocumentResponseDto? Document { get; init; }

    public static CreateEmployeeDocumentResult NotFoundEmployee() => new() { Error = CreateEmployeeDocumentError.EmployeeNotFound };
    public static CreateEmployeeDocumentResult NotFoundDocumentType() => new() { Error = CreateEmployeeDocumentError.DocumentTypeNotFound };
    public static CreateEmployeeDocumentResult Ok(EmployeeDocumentResponseDto document) => new() { Document = document };
}

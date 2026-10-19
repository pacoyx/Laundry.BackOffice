using System.ComponentModel.DataAnnotations;

public class EmployeeDocument
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public int DocumentTypeId { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BlobUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Employee? Employee { get; set; }

    public EmployeeDocumentType? DocumentType { get; set; }
}

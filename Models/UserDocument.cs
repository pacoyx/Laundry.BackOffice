using System.ComponentModel.DataAnnotations;

public class UserDocument
{
    public long Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(100)]
    public string DocumentName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BlobUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}


public class SharedDocument
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string BlobUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int UploadedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User UploadedByUser { get; set; } = null!;

    public ICollection<SharedDocumentRecipient> Recipients { get; set; } = new List<SharedDocumentRecipient>();
}

public class PickupGuideRevisionPhoto
{
    public long Id { get; set; }

    public long PickupGuideRevisionId { get; set; }

    public PickupGuideRevision PickupGuideRevision { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }
}

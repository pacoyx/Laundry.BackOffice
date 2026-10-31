public class FixedAssetMaintenanceAttachment
{
    public long Id { get; set; }

    public long FixedAssetMaintenanceId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }

    public FixedAssetMaintenance FixedAssetMaintenance { get; set; } = null!;
}

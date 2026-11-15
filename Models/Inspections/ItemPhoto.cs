
public class ItemPhoto
{
    public long Id { get; set; }

    public int WorkGuideMainId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public string BlobUrl { get; set; } = string.Empty;

    /// <summary>
    /// Ruta de la miniatura (~320 px, JPEG) usada en listados. Null en fotos antiguas: se genera la primera
    /// vez que se pide la miniatura.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public WorkGuideEvidenceStage Stage { get; set; } = WorkGuideEvidenceStage.Revision;

    public long? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime CreatedAt { get; set; }

    public WorkGuideMain WorkGuideMain { get; set; } = null!;
}

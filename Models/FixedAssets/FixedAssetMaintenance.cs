using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FixedAssetMaintenance
{
    public long Id { get; set; }

    public long FixedAssetId { get; set; }

    public DateTime Date { get; set; }

    public FixedAssetMaintenanceType Type { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [MaxLength(500)]
    public string? Observations { get; set; }

    public DateTime? NextMaintenanceDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;

    public ICollection<FixedAssetMaintenanceAttachment> Attachments { get; set; } = new List<FixedAssetMaintenanceAttachment>();
}

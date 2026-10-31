using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FixedAsset
{
    public long Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public DateTime AcquisitionDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AcquisitionCost { get; set; }

    public int? SupplierId { get; set; }

    public int? PurchaseInvoiceId { get; set; }

    public int BranchId { get; set; }

    public int? ResponsibleUserId { get; set; }

    public int? UsefulLifeYearsOverride { get; set; }

    public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Active;

    public DateTime? DisposalDate { get; set; }

    [MaxLength(300)]
    public string? DisposalReason { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public FixedAssetCategory Category { get; set; } = null!;

    public Supplier? Supplier { get; set; }

    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public BranchSales Branch { get; set; } = null!;

    public User? ResponsibleUser { get; set; }

    public ICollection<FixedAssetMaintenance> Maintenances { get; set; } = new List<FixedAssetMaintenance>();

    public ICollection<FixedAssetReassignment> Reassignments { get; set; } = new List<FixedAssetReassignment>();
}

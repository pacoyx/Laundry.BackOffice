using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PickupGuide
{
    public long Id { get; set; }

    [MaxLength(40)]
    public string GuideNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhysicalGuideNumber { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int BranchSalesId { get; set; }

    public BranchSales BranchSales { get; set; } = null!;

    public DateTime PickupDate { get; set; }

    public PickupGuideStatus Status { get; set; } = PickupGuideStatus.Draft;

    [MaxLength(500)]
    public string? Observation { get; set; }

    public long CustomerPriceListId { get; set; }

    public CustomerPriceList CustomerPriceList { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public int? VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public PickupGuideEmailStatus EmailDeliveryStatus { get; set; } = PickupGuideEmailStatus.Pending;

    public DateTime? EmailSentAt { get; set; }

    public DateTime? LastEmailAttemptAt { get; set; }

    public int EmailRetryCount { get; set; }

    [MaxLength(1000)]
    public string? LastEmailError { get; set; }

    public ICollection<PickupGuideDetail> Details { get; set; } = new List<PickupGuideDetail>();

    public ICollection<PickupGuideAttachment> Attachments { get; set; } = new List<PickupGuideAttachment>();

    public ICollection<PickupGuideRevision> Revisions { get; set; } = new List<PickupGuideRevision>();
}

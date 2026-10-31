using System.ComponentModel.DataAnnotations;

public class FixedAssetReassignment
{
    public long Id { get; set; }

    public long FixedAssetId { get; set; }

    public DateTime Date { get; set; }

    public int? PreviousBranchId { get; set; }

    public int NewBranchId { get; set; }

    public int? PreviousResponsibleUserId { get; set; }

    public int? NewResponsibleUserId { get; set; }

    [MaxLength(300)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;

    public BranchSales? PreviousBranch { get; set; }

    public BranchSales NewBranch { get; set; } = null!;

    public User? PreviousResponsibleUser { get; set; }

    public User? NewResponsibleUser { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;
using DexterCompany.Models;

public class WorkGuideDetailChangeLog
{
    public long Id { get; set; }

    public int WorkGuideMainId { get; set; }
    public WorkGuideMain WorkGuideMain { get; set; } = null!;

    public int? WorkGuideDetailId { get; set; }
    public WorkGuideDetail? WorkGuideDetail { get; set; }

    public WorkGuideDetailChangeType ChangeType { get; set; }

    public int ServiceItemId { get; set; }
    public ProdService ServiceItem { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldQuantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NewQuantity { get; set; }

    public string? Observation { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PickupGuideDetailRevision
{
    public long Id { get; set; }

    public long PickupGuideRevisionId { get; set; }

    public PickupGuideRevision PickupGuideRevision { get; set; } = null!;

    public long PickupGuideDetailId { get; set; }

    public PickupGuideDetail PickupGuideDetail { get; set; } = null!;

    public PickupGuideRevisionActionType ActionType { get; set; } = PickupGuideRevisionActionType.QuantityAdjustment;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalQuantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NewQuantity { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

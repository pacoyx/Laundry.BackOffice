using System.ComponentModel.DataAnnotations;

public class PickupGuideRevision
{
    public long Id { get; set; }

    public long PickupGuideId { get; set; }

    public PickupGuide PickupGuide { get; set; } = null!;

    public long EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [MaxLength(500)]
    public string Observation { get; set; } = string.Empty;

    public DateTime RevisionDate { get; set; }

    public ICollection<PickupGuideDetailRevision> DetailRevisions { get; set; } = new List<PickupGuideDetailRevision>();

    public ICollection<PickupGuideRevisionPhoto> Photos { get; set; } = new List<PickupGuideRevisionPhoto>();
}

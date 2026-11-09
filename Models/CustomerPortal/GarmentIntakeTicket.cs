using System.ComponentModel.DataAnnotations;

public class GarmentIntakeTicket : BaseAuditableEntity
{
    public int Id { get; set; }

    [MaxLength(40)]
    public string TicketNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public long CreatedByCustomerUserId { get; set; }

    public CustomerUser CreatedByCustomerUser { get; set; } = null!;

    public GarmentIntakeTicketStatus Status { get; set; } = GarmentIntakeTicketStatus.Pending;

    [MaxLength(500)]
    public string? VoidedReason { get; set; }

    public DateTime? VoidedAt { get; set; }

    public long? VoidedByCustomerUserId { get; set; }

    public CustomerUser? VoidedByCustomerUser { get; set; }

    public long? PickupGuideId { get; set; }

    public PickupGuide? PickupGuide { get; set; }

    public DateTime? PickedUpAt { get; set; }

    public ICollection<GarmentIntakeTicketDetail> Details { get; set; } = new List<GarmentIntakeTicketDetail>();

    public ICollection<GarmentIntakeTicketPhoto> Photos { get; set; } = new List<GarmentIntakeTicketPhoto>();
}

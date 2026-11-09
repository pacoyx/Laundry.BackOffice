using System.ComponentModel.DataAnnotations;

public class GarmentIntakeTicketPhoto
{
    public long Id { get; set; }

    public int TicketId { get; set; }

    public GarmentIntakeTicket Ticket { get; set; } = null!;

    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string OriginalFileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }

    public long CreatedByCustomerUserId { get; set; }

    public CustomerUser CreatedByCustomerUser { get; set; } = null!;
}

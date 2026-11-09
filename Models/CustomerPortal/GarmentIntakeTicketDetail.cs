using System.ComponentModel.DataAnnotations;
using DexterCompany.Models;

public class GarmentIntakeTicketDetail
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public GarmentIntakeTicket Ticket { get; set; } = null!;

    public int WorkerId { get; set; }

    public CompanyWorker Worker { get; set; } = null!;

    public int ServiceItemId { get; set; }

    public ProdService ServiceItem { get; set; } = null!;

    public int Quantity { get; set; }

    [MaxLength(500)]
    public string? Observations { get; set; }
}

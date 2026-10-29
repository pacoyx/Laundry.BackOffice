using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class WebOrder
{
    public long Id { get; set; }

    [MaxLength(40)]
    public string OrderNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public WebOrderStatus Status { get; set; } = WebOrderStatus.Pendiente;

    [MaxLength(200)]
    public string DeliveryAddress { get; set; } = string.Empty;

    public int DistrictId { get; set; }
    public District District { get; set; } = null!;

    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    public long ZonePriceListId { get; set; }
    public ZonePriceList ZonePriceList { get; set; } = null!;

    public DateTime? RequestedPickupDate { get; set; }

    public DateTime? ScheduledPickupDate { get; set; }

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public long? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? WorkGuideMainId { get; set; }
    public WorkGuideMain? WorkGuideMain { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(500)]
    public string? PickupObservation { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public int? ConfirmedByUserId { get; set; }
    public User? ConfirmedByUser { get; set; }

    public ICollection<WebOrderDetail> Details { get; set; } = new List<WebOrderDetail>();
}

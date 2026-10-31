
public class GuiaRemision
{
    public long Id { get; set; }

    public long InvoiceId { get; set; }

    public string Serie { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaTraslado { get; set; }

    public string MotivoTraslado { get; set; } = "Venta";

    public string DriverName { get; set; } = string.Empty;

    public string DriverDni { get; set; } = string.Empty;

    public string OriginAddress { get; set; } = string.Empty;

    public string DeliveryAddress { get; set; } = string.Empty;

    public int? VehicleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public Vehicle? Vehicle { get; set; }
}

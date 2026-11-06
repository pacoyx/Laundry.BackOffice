public class SneakerReportFilter
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int? BranchId { get; set; }

    public int? BrandId { get; set; }

    public int? CustomerId { get; set; }

    public string? TicketStatus { get; set; }

    public bool? PendingDelivery { get; set; }
}

// Una fila por zapatilla, usada para el detalle que se ve al expandir un grupo.
public record SneakerReportItemResponse(
    long SneakerDetailId,
    string? Brand,
    string? PrimaryColor,
    bool HasLaces,
    bool HasInsoles
);

// Una fila por guia (agrupa todas sus zapatillas). Es lo que renderiza la tabla principal.
public record SneakerReportGroupResponse(
    int WorkGuideId,
    string WorkGuideNumber,
    DateTime WorkGuideDate,
    string CustomerName,
    int SneakerCount,
    string DeliveryStatus,
    int PhotoCount,
    List<SneakerReportItemResponse> Items
);

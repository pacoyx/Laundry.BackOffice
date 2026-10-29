public record WebOrderAdminSummaryResponse(
    long Id,
    string OrderNumber,
    WebOrderStatus Status,
    string CustomerName,
    string ContactPhone,
    string DeliveryAddress,
    decimal Total,
    DateTime CreatedAt,
    DateTime? RequestedPickupDate,
    DateTime? ScheduledPickupDate);

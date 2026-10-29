public record WebOrderSummaryResponse(
    long Id,
    string OrderNumber,
    WebOrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    DateTime? ScheduledPickupDate);

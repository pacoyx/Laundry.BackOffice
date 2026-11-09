public record WebOrderTrackResponse(
    string OrderNumber,
    WebOrderStatus Status,
    DateTime? ScheduledPickupDate,
    DateTime CreatedAt
);

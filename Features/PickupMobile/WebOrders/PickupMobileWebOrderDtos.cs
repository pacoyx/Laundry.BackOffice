public record PickupMobileWebOrderDto(
    long Id,
    string OrderNumber,
    string DeliveryAddress,
    string ContactPhone,
    DateTime? ScheduledPickupDate,
    WebOrderStatus Status,
    string? WorkGuideSerie,
    string? WorkGuideNumero);

public record CompletePickupMobileWebOrderRequest(string? Observation);

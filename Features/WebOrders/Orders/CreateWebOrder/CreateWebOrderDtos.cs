public record CreateWebOrderItemRequest(int ServiceItemId, decimal Quantity);

public record CreateWebOrderRequest(
    string? GuestName,
    string? GuestPhone,
    string DeliveryAddress,
    int DistrictId,
    string ContactPhone,
    DateTime? RequestedPickupDate,
    List<CreateWebOrderItemRequest> Items);

public record WebOrderDetailResponse(int ServiceItemId, string Name, decimal Quantity, decimal UnitPrice, decimal Total);

public record WebOrderResponse(
    long Id,
    string OrderNumber,
    WebOrderStatus Status,
    string DeliveryAddress,
    int DistrictId,
    string ContactPhone,
    DateTime? RequestedPickupDate,
    DateTime? ScheduledPickupDate,
    decimal Total,
    DateTime CreatedAt,
    string? WorkGuideSerie,
    string? WorkGuideNumero,
    List<WebOrderDetailResponse> Details);

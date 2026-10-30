public record WebOrderEvidencePhotoResponse(long Id, string BlobUrl, WorkGuideEvidenceStage Stage, DateTime CreatedAt);

public record WebOrderDetailFullResponse(
    long Id,
    string OrderNumber,
    WebOrderStatus Status,
    string DeliveryAddress,
    string ContactPhone,
    DateTime? RequestedPickupDate,
    DateTime? ScheduledPickupDate,
    string? AdditionalItemsNote,
    decimal Total,
    DateTime CreatedAt,
    string? WorkGuideSerie,
    string? WorkGuideNumero,
    List<WebOrderDetailResponse> Details,
    List<WebOrderEvidencePhotoResponse> Photos);

public record GarmentTicketServiceItemLookupDto(int ServiceItemId, string Name);

public record GarmentIntakeTicketDetailRequest(int WorkerId, int ServiceItemId, int Quantity, string? Observations);

public record GarmentIntakeTicketDetailResponse(
    int Id,
    int WorkerId,
    string WorkerName,
    int ServiceItemId,
    string ServiceItemName,
    int Quantity,
    string? Observations
);

public record GarmentIntakeTicketPhotoResponse(
    long Id,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt
);

public record GarmentIntakeTicketResponse(
    int Id,
    string TicketNumber,
    GarmentIntakeTicketStatus Status,
    DateTime Created,
    string CreatedByUserName,
    string? VoidedReason,
    DateTime? VoidedAt,
    string? VoidedByUserName,
    long? PickupGuideId,
    DateTime? PickedUpAt,
    List<GarmentIntakeTicketDetailResponse> Details,
    List<GarmentIntakeTicketPhotoResponse> Photos
);

public record GarmentIntakeTicketSummaryResponse(
    int Id,
    string TicketNumber,
    GarmentIntakeTicketStatus Status,
    DateTime Created,
    int TotalQuantity,
    int LineCount
);

public record GarmentIntakeTicketPendingSummaryItem(int ServiceItemId, string ServiceItemName, int TotalQuantity);

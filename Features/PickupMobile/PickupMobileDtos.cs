public record PickupMobileCustomerLookupDto(int Id, string BusinessName);

public record PickupMobileVehicleLookupDto(int Id, string Plate, string Description);

public record PickupMobileEmployeeLookupDto(long Id, string FullName);

public record PickupMobileServiceItemLookupDto(int Id, string Name, decimal UnitPrice);

public record PickupMobileGuideDto(
    long Id,
    string GuideNumber,
    string? PhysicalGuideNumber,
    string Customer,
    DateTime Date,
    PickupGuideStatus Status,
    bool HasPhotos,
    bool HasSignature
);

public record PickupMobileAttachmentDto(
    long Id,
    string AttachmentType,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt
);

public record PickupMobileDashboardResponse(
    int TodayPickupGuides,
    int DraftPickupGuides
);

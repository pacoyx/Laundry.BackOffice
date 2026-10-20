public record PortalPickupGuideDetailResponse(
    long Id,
    int ServiceItemId,
    string ServiceItemName,
    decimal Quantity,
    decimal? Weight,
    decimal UnitPrice,
    decimal Total,
    string? Observation,
    decimal OriginalQuantity,
    decimal ConfirmedQuantity
);

public record PortalPickupGuideSummaryResponse(
    long Id,
    string GuideNumber,
    int BranchSalesId,
    DateTime PickupDate,
    PickupGuideStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    bool HasPdf,
    bool HasPhotos,
    bool HasSignature,
    bool HasRevisions
);

public record PortalEmployeeSummaryResponse(
    long Id,
    string FullName
);

public record PortalVehicleSummaryResponse(
    int Id,
    string Plate,
    string Description
);

public record PortalPickupGuideResponse(
    long Id,
    string GuideNumber,
    int BranchSalesId,
    DateTime PickupDate,
    PickupGuideStatus Status,
    string? Observation,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<PortalPickupGuideDetailResponse> Details,
    PortalEmployeeSummaryResponse? Employee,
    PortalVehicleSummaryResponse? Vehicle
);

public record PortalPickupGuideRevisionItemResponse(
    PickupGuideRevisionActionType ActionType,
    string ServiceName,
    decimal OriginalQuantity,
    decimal NewQuantity,
    string Reason
);

public record PortalPickupGuideRevisionPhotoResponse(
    long Id,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt
);

public record PortalPickupGuideRevisionResponse(
    DateTime RevisionDate,
    string EmployeeName,
    string Observation,
    List<PortalPickupGuideRevisionItemResponse> Items,
    List<PortalPickupGuideRevisionPhotoResponse> Photos
);

public record PortalPickupGuideAttachmentResponse(
    long Id,
    PickupGuideAttachmentType AttachmentType,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt
);

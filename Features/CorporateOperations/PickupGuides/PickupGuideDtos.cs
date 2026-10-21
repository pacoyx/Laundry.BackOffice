public record PickupGuideDetailRequest(int ServiceItemId, decimal Quantity, decimal? Weight, string? Observation);

public record PickupGuideDetailResponse(
    long Id,
    int ServiceItemId,
    string ServiceItemName,
    decimal Quantity,
    decimal? Weight,
    decimal UnitPrice,
    decimal Total,
    string? Observation
);

public record PickupGuideResponse(
    long Id,
    string GuideNumber,
    string? PhysicalGuideNumber,
    int CustomerId,
    string CustomerName,
    int BranchSalesId,
    DateTime PickupDate,
    PickupGuideStatus Status,
    string? Observation,
    long CustomerPriceListId,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    long? EmployeeId,
    string? EmployeeName,
    int? VehicleId,
    string? VehiclePlate,
    string? VehicleDescription,
    PickupGuideEmailStatus EmailDeliveryStatus,
    DateTime? EmailSentAt,
    List<PickupGuideDetailResponse> Details,
    List<PickupGuideAttachmentResponse> Attachments,
    List<PickupGuideRevisionResponse> Revisions
);

public record PickupGuideSummaryResponse(
    long Id,
    string GuideNumber,
    string? PhysicalGuideNumber,
    int CustomerId,
    string CustomerName,
    int BranchSalesId,
    DateTime PickupDate,
    PickupGuideStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt
);

public record CreatePickupGuideRequest(
    int CustomerId,
    int BranchSalesId,
    DateTime PickupDate,
    string? Observation,
    string? PhysicalGuideNumber,
    long? EmployeeId,
    int? VehicleId,
    List<PickupGuideDetailRequest> Details
);

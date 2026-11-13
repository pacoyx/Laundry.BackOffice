public record CreateFixedAssetMaintenanceRequest(
    DateTime Date,
    FixedAssetMaintenanceType Type,
    string Description,
    decimal Cost,
    string? Observations,
    DateTime? NextMaintenanceDate,
    string? ServiceProvider = null,
    bool PutAssetOutOfService = false,
    string? OutOfServiceReason = null,
    DateTime? EstimatedEndDate = null);

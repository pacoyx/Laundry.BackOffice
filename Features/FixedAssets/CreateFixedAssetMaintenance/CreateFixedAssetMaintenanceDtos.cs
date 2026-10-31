public record CreateFixedAssetMaintenanceRequest(
    DateTime Date,
    FixedAssetMaintenanceType Type,
    string Description,
    decimal Cost,
    string? Observations,
    DateTime? NextMaintenanceDate);

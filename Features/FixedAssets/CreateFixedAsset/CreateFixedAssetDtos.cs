public record CreateFixedAssetRequest(
    string Name,
    string? Description,
    int CategoryId,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    int? SupplierId,
    int? PurchaseInvoiceId,
    int BranchId,
    int? ResponsibleUserId,
    int? UsefulLifeYearsOverride,
    string? Notes);

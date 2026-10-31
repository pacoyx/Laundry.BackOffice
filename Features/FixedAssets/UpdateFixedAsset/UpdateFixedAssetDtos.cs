public record UpdateFixedAssetRequest(
    string Name,
    string? Description,
    int CategoryId,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    int? SupplierId,
    int? PurchaseInvoiceId,
    int? UsefulLifeYearsOverride,
    string? Notes);

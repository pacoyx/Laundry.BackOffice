public record FixedAssetCategoryResponse(
    int Id,
    string Name,
    decimal DefaultDepreciationRatePercent,
    int DefaultUsefulLifeYears);

public record FixedAssetListItemResponse(
    long Id,
    string Code,
    string Name,
    int CategoryId,
    string CategoryName,
    int BranchId,
    string BranchName,
    int? ResponsibleUserId,
    string? ResponsibleUserName,
    FixedAssetStatus Status,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    decimal BookValue);

public record FixedAssetDetailResponse(
    long Id,
    string Code,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    int? SupplierId,
    string? SupplierName,
    int? PurchaseInvoiceId,
    string? PurchaseInvoiceNumber,
    int BranchId,
    string BranchName,
    int? ResponsibleUserId,
    string? ResponsibleUserName,
    int UsefulLifeYears,
    FixedAssetStatus Status,
    DateTime? DisposalDate,
    string? DisposalReason,
    string? Notes,
    decimal BookValue,
    List<FixedAssetMaintenanceResponse> Maintenances,
    List<FixedAssetReassignmentResponse> Reassignments);

public record FixedAssetMaintenanceAttachmentResponse(
    long Id,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt);

public record FixedAssetMaintenanceResponse(
    long Id,
    DateTime Date,
    FixedAssetMaintenanceType Type,
    string Description,
    decimal Cost,
    string? Observations,
    DateTime? NextMaintenanceDate,
    List<FixedAssetMaintenanceAttachmentResponse> Attachments);

public record FixedAssetReassignmentResponse(
    long Id,
    DateTime Date,
    int? PreviousBranchId,
    string? PreviousBranchName,
    int NewBranchId,
    string NewBranchName,
    int? PreviousResponsibleUserId,
    string? PreviousResponsibleUserName,
    int? NewResponsibleUserId,
    string? NewResponsibleUserName,
    string? Reason);

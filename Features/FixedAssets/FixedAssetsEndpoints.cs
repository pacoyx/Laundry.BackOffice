public static class FixedAssetsEndpoints
{
    public static void MapFixedAssets(this IEndpointRouteBuilder app)
    {
        var categoriesGroup = app.MapGroup("/api/fixed-asset-categories")
            .WithTags("FixedAssetCategories")
            .RequirePermission(PermissionCodes.FixedAssets);
        categoriesGroup.MapGetFixedAssetCategories();

        var group = app.MapGroup("/api/fixed-assets")
            .WithTags("FixedAssets")
            .RequirePermission(PermissionCodes.FixedAssets);

        group.MapCreateFixedAsset();
        group.MapGetFixedAssets();
        group.MapGetFixedAssetDetail();
        group.MapUpdateFixedAsset();
        group.MapReassignFixedAsset();
        group.MapDisposeFixedAsset();
        group.MapCreateFixedAssetMaintenance();
        group.MapCreateFixedAssetMaintenanceAttachment();
        group.MapDownloadFixedAssetMaintenanceAttachment();
    }

    internal static FixedAssetListItemResponse ToListItemResponse(FixedAsset asset) => new(
        asset.Id,
        asset.Code,
        asset.Name,
        asset.CategoryId,
        asset.Category.Name,
        asset.BranchId,
        asset.Branch.Description,
        asset.ResponsibleUserId,
        asset.ResponsibleUser?.Name,
        asset.Status,
        asset.AcquisitionDate,
        asset.AcquisitionCost,
        FixedAssetDepreciationCalculator.GetBookValue(asset)
    );

    internal static FixedAssetDetailResponse ToDetailResponse(FixedAsset asset) => new(
        asset.Id,
        asset.Code,
        asset.Name,
        asset.Description,
        asset.CategoryId,
        asset.Category.Name,
        asset.AcquisitionDate,
        asset.AcquisitionCost,
        asset.SupplierId,
        asset.Supplier?.Name,
        asset.PurchaseInvoiceId,
        asset.PurchaseInvoice is null ? null : $"{asset.PurchaseInvoice.InvoiceSerie}-{asset.PurchaseInvoice.InvoiceNumber}",
        asset.BranchId,
        asset.Branch.Description,
        asset.ResponsibleUserId,
        asset.ResponsibleUser?.Name,
        FixedAssetDepreciationCalculator.GetEffectiveUsefulLifeYears(asset),
        asset.Status,
        asset.DisposalDate,
        asset.DisposalReason,
        asset.Notes,
        FixedAssetDepreciationCalculator.GetBookValue(asset),
        asset.Maintenances
            .OrderByDescending(m => m.Date)
            .Select(ToMaintenanceResponse)
            .ToList(),
        asset.Reassignments
            .OrderByDescending(r => r.Date)
            .Select(ToReassignmentResponse)
            .ToList()
    );

    internal static FixedAssetMaintenanceResponse ToMaintenanceResponse(FixedAssetMaintenance maintenance) => new(
        maintenance.Id,
        maintenance.Date,
        maintenance.Type,
        maintenance.Description,
        maintenance.Cost,
        maintenance.Observations,
        maintenance.NextMaintenanceDate,
        maintenance.Attachments.Select(a => new FixedAssetMaintenanceAttachmentResponse(
            a.Id, a.OriginalFileName, a.ContentType, a.FileSize, a.CreatedAt
        )).ToList()
    );

    internal static FixedAssetReassignmentResponse ToReassignmentResponse(FixedAssetReassignment reassignment) => new(
        reassignment.Id,
        reassignment.Date,
        reassignment.PreviousBranchId,
        reassignment.PreviousBranch?.Description,
        reassignment.NewBranchId,
        reassignment.NewBranch.Description,
        reassignment.PreviousResponsibleUserId,
        reassignment.PreviousResponsibleUser?.Name,
        reassignment.NewResponsibleUserId,
        reassignment.NewResponsibleUser?.Name,
        reassignment.Reason
    );
}

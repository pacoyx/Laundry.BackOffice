public static class PickupGuidesEndpoints
{
    public static void MapPickupGuides(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pickup-guides")
            .WithTags("PickupGuides")
            .RequirePermission(PermissionCodes.CorporateOperations);

        group.MapCreatePickupGuide();
        group.MapGetPickupGuides();
        group.MapGetPickupGuide();
        group.MapUpdatePickupGuide();
        group.MapDeletePickupGuide();
        group.MapChangePickupGuideStatus();
        group.MapGetPickupGuidesByCustomerReport();
        group.MapGetPickupGuidesMonthlyReport();

        group.MapCreateAttachment();
        group.MapGetAttachments();
        group.MapDeleteAttachment();
        group.MapDownloadAttachment();

        group.MapCreatePickupGuideRevision();
        group.MapGetPickupGuideRevisions();
        group.MapCreatePickupGuideRevisionPhoto();
        group.MapGetPickupGuideRevisionPhotos();
        group.MapDownloadPickupGuideRevisionPhoto();

        group.MapResendPickupGuideEmail();
    }

    internal static string GetCustomerDisplayName(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.BusinessName
            : $"{customer.FirtsName} {customer.LastName}".Trim();

    internal static PickupGuideDetailResponse ToDetailResponse(PickupGuideDetail detail) => new(
        detail.Id, detail.ServiceItemId, detail.ServiceItem.Name, detail.Quantity, detail.Weight,
        detail.UnitPrice, detail.Total, detail.Observation
    );

    internal static PickupGuideResponse ToResponse(PickupGuide guide) => new(
        guide.Id, guide.GuideNumber, guide.PhysicalGuideNumber, guide.CustomerId, GetCustomerDisplayName(guide.Customer),
        guide.BranchSalesId, guide.PickupDate, guide.Status, guide.Observation,
        guide.CustomerPriceListId, guide.TotalAmount, guide.CreatedAt, guide.UpdatedAt,
        guide.EmployeeId, guide.Employee is null ? null : $"{guide.Employee.FirstName} {guide.Employee.LastName}".Trim(),
        guide.VehicleId, guide.Vehicle?.Plate, guide.Vehicle?.Description,
        guide.EmailDeliveryStatus, guide.EmailSentAt,
        guide.Details.Select(ToDetailResponse).ToList(),
        guide.Attachments.Select(PickupGuideAttachmentsEndpoints.ToResponse).ToList(),
        guide.Revisions.Select(PickupGuideRevisionsEndpoints.ToResponse).ToList()
    );

    internal static PickupGuideSummaryResponse ToSummaryResponse(PickupGuide guide) => new(
        guide.Id, guide.GuideNumber, guide.PhysicalGuideNumber, guide.CustomerId, GetCustomerDisplayName(guide.Customer),
        guide.BranchSalesId, guide.PickupDate, guide.Status, guide.TotalAmount, guide.CreatedAt
    );
}

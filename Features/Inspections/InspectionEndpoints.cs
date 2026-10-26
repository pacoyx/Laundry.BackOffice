public static class InspectionEndpoints
{
    public static void MapInspections(this IEndpointRouteBuilder app)
    {
        var detailGroup = app.MapGroup("/api/workguide-details").WithTags("Inspections").RequirePermission(PermissionCodes.ReceptionMaestros);
        detailGroup.MapCreateItemInspection();
        detailGroup.MapGetItemInspection();

        var workGuideGroup = app.MapGroup("/api/workguides").WithTags("WorkGuideInspections").RequirePermission(PermissionCodes.ReceptionMaestros);
        workGuideGroup.MapGetInspectionSummary();
        workGuideGroup.MapGetWorkGuidePhotos();
        workGuideGroup.MapCreateItemPhoto();
        workGuideGroup.MapDownloadItemPhoto();
    }
}

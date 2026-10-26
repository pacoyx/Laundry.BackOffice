public static class CollectionGuideEndpoint
{
    public static void MapCollectionGuide(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/collectionGuide").RequirePermission(PermissionCodes.ReceptionEmision);
        group.MapCreateCollectionGuide();
    }
}
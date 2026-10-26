public static class CollectionWorkerEndpoint
{
    public static void MapClothingWorkerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clothingWorker").RequirePermission(PermissionCodes.ReceptionMaestros);
        group.MapGetCollectionWorkerById();
    }
}
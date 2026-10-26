public static class ClothingItemEndpoint
{
    public static void MapClothingItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clothingItem").RequirePermission(PermissionCodes.ReceptionMaestros);
        group.MapGetClothingItems();
    }
}
public static class CustomerGarmentHistoryEndpoints
{
    public static void MapCustomerGarmentHistory(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customerGarmentHistory").RequirePermission(PermissionCodes.Reception);
        group.MapGetCustomerGarmentHistory();
        group.MapDeleteCustomerGarmentHistory();
    }
}

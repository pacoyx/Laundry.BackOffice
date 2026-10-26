public static class CustomerGarmentHistoryEndpoints
{
    public static void MapCustomerGarmentHistory(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customerGarmentHistory").RequirePermission(PermissionCodes.ReceptionConsultas);
        group.MapGetCustomerGarmentHistory();
        group.MapDeleteCustomerGarmentHistory();
    }
}

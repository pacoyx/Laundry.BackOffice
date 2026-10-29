public static class WebOrdersEndpoints
{
    public static void MapWebOrders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/web-orders").WithTags("WebOrders");

        group.MapRequestOtp();
        group.MapVerifyOtp();
        group.MapRefreshWebCustomerToken();

        group.MapGetZones();
        group.MapGetDistricts();
        group.MapGetActiveZonePriceList();

        group.MapCreateWebOrder();

        var meGroup = app.MapGroup("/api/web-orders/me")
            .WithTags("WebOrders")
            .RequireAuthorization("WebCustomerPortalAccess");

        meGroup.MapGetMyWebOrders();
        meGroup.MapGetMyWebOrderDetail();

        var adminGroup = app.MapGroup("/api/web-orders/admin").WithTags("WebOrders");

        adminGroup.MapGetWebOrdersAdmin();
        adminGroup.MapConfirmWebOrder();
        adminGroup.MapRejectWebOrder();
        adminGroup.MapScheduleWebOrder();

        // Maestros: distritos y tarifas por zona (reception.maestros.access, igual que el resto de Maestros).
        adminGroup.MapGetDistrictsAdmin();
        adminGroup.MapCreateDistrict();
        adminGroup.MapUpdateDistrict();

        adminGroup.MapGetZonePriceLists();
        adminGroup.MapGetZonePriceList();
        adminGroup.MapCreateZonePriceList();
        adminGroup.MapUpdateZonePriceList();
        adminGroup.MapActivateZonePriceList();
        adminGroup.MapDeactivateZonePriceList();
    }
}

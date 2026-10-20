public static class CustomerPortalEndpoints
{
    public static void MapCustomerPortal(this IEndpointRouteBuilder app)
    {
        var anonymousGroup = app.MapGroup("/api/customer-portal").WithTags("CustomerPortal");
        anonymousGroup.MapLoginCustomer();
        anonymousGroup.MapRefreshCustomerToken();
        anonymousGroup.MapForgotPassword();
        anonymousGroup.MapResetPassword();
        anonymousGroup.MapActivateAccount();

        var portalGroup = app.MapGroup("/api/customer-portal")
            .WithTags("CustomerPortal")
            .RequireAuthorization("CustomerPortalAccess");

        portalGroup.MapGetMe();

        portalGroup.MapGetCustomerRoles();
        portalGroup.MapGetCustomerUsers();
        portalGroup.MapCreateCustomerUser();
        portalGroup.MapUpdateCustomerUser();
        portalGroup.MapDeactivateCustomerUser();
        portalGroup.MapGenerateActivationLink();
        portalGroup.MapResetCustomerUserPassword();

        portalGroup.MapGetPortalPickupGuides();
        portalGroup.MapGetPortalPickupGuide();
        portalGroup.MapGetPortalPickupGuideAttachments();
        portalGroup.MapDownloadPortalPickupGuideAttachment();
        portalGroup.MapGetPortalPickupGuideRevisions();
        portalGroup.MapDownloadPortalPickupGuideRevisionPhoto();

        portalGroup.MapGetPortalInvoices();
        portalGroup.MapGetPortalInvoice();

        portalGroup.MapGetMonthlySummary();
        portalGroup.MapGetHistory();
        portalGroup.MapGetByService();
    }
}

public static class GuiasRemisionEndpoints
{
    public static void MapGuiasRemision(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/guias-remision")
            .WithTags("GuiasRemision")
            .RequirePermission(PermissionCodes.GuiaRemision);

        group.MapCreateGuiaRemision();
        group.MapGetGuiasRemision();
        group.MapGetGuiaRemisionByInvoice();
        group.MapGetGuiaRemisionPdf();
    }

    internal static GuiaRemisionResponse ToResponse(GuiaRemision guia) => new(
        guia.Id,
        guia.InvoiceId,
        guia.Invoice.Serie,
        guia.Invoice.Numero,
        guia.Serie,
        guia.Numero,
        guia.FechaEmision,
        guia.FechaTraslado,
        guia.MotivoTraslado,
        guia.DriverName,
        guia.DriverDni,
        guia.OriginAddress,
        guia.DeliveryAddress,
        guia.VehicleId,
        guia.Vehicle?.Plate,
        GetCustomerName(guia.Invoice.Customer),
        GetCustomerTaxId(guia.Invoice.Customer)
    );

    private static string GetCustomerName(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.BusinessName
            : $"{customer.FirtsName} {customer.LastName}".Trim();

    private static string GetCustomerTaxId(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.TaxId
            : customer.DocPersonal ?? "-";
}

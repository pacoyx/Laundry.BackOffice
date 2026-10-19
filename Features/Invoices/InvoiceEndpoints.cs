public static class InvoiceEndpoints
{
    public static void MapInvoices(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices").RequirePermission(PermissionCodes.Invoices);
        group.MapCreateInvoice();
        group.MapGetInvoice();
        group.MapGetInvoicePdf();
        group.MapGetInvoices();
        group.MapCancelInvoice();
        group.MapRegisterPayment();
        group.MapGetPayments();
        group.MapValuationReport();
    }
}

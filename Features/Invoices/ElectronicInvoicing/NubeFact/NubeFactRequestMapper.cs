public static class NubeFactRequestMapper
{
    private const string FacturaDocId = "01";
    private const string BoletaDocId = "03";

    public static bool TryGetTipoDeComprobante(string docId, out int tipoDeComprobante)
    {
        switch (docId)
        {
            case FacturaDocId:
                tipoDeComprobante = 1;
                return true;
            case BoletaDocId:
                tipoDeComprobante = 2;
                return true;
            default:
                tipoDeComprobante = 0;
                return false;
        }
    }

    public static NubeFactInvoiceRequest Build(
        Invoice invoice, int tipoDeComprobante, decimal igvRate, IReadOnlyDictionary<int, Product> products)
    {
        var customer = invoice.Customer;

        return new NubeFactInvoiceRequest
        {
            TipoDeComprobante = tipoDeComprobante,
            Serie = invoice.Serie,
            // NubeFact exige "numero" como entero; nuestro Numero es string con ceros a la
            // izquierda (ej. "000123"), por eso se parsea.
            Numero = int.Parse(invoice.Numero),
            ClienteTipoDeDocumento = GetClienteTipoDeDocumento(customer),
            ClienteNumeroDeDocumento = GetCustomerTaxId(customer),
            ClienteDenominacion = GetCustomerName(customer),
            ClienteDireccion = GetCustomerAddress(customer),
            ClienteEmail = customer.Email ?? string.Empty,
            FechaDeEmision = invoice.IssueDate,
            FechaDeVencimiento = invoice.DueDate,
            Moneda = invoice.CurrencyId == "USD" ? 2 : 1,
            PorcentajeDeIgv = (double)(igvRate * 100),
            TotalGravada = (double)invoice.SubTotal,
            TotalIgv = (double)invoice.Igv,
            Total = (double)invoice.Total,
            Observaciones = invoice.Notes ?? string.Empty,
            Items = invoice.Details.Select(d => BuildItem(d, igvRate, products)).ToList()
        };
    }

    private static NubeFactItem BuildItem(InvoiceDetail detail, decimal igvRate, IReadOnlyDictionary<int, Product> products)
    {
        // InvoiceDetail.UnitPrice es SIN IGV (ver InvoiceSettings.IgvRate / CreateInvoiceEndpoint).
        var precioUnitario = detail.UnitPrice * (1 + igvRate);
        var codeSunat = products.TryGetValue(detail.ProductId, out var product) ? product.UnitMeasurement?.CodeSunat : null;

        return new NubeFactItem
        {
            UnidadDeMedida = string.IsNullOrWhiteSpace(codeSunat) ? "NIU" : codeSunat,
            Codigo = detail.ProductId.ToString(),
            Descripcion = detail.Description,
            Cantidad = (double)detail.Quantity,
            ValorUnitario = (double)detail.UnitPrice,
            PrecioUnitario = (double)precioUnitario,
            Subtotal = (double)(detail.Total - Math.Round(detail.Total * igvRate / (1 + igvRate), 2)),
            Igv = (double)Math.Round(detail.Total * igvRate / (1 + igvRate), 2),
            Total = (double)detail.Total
        };
    }

    private static int GetClienteTipoDeDocumento(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? 6 // RUC
            : 1; // DNI

    private static string GetCustomerName(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.BusinessName
            : $"{customer.FirtsName} {customer.LastName}".Trim();

    private static string GetCustomerTaxId(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.TaxId
            : customer.DocPersonal ?? string.Empty;

    private static string GetCustomerAddress(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.FiscalAddress
            : customer.Address ?? string.Empty;
}

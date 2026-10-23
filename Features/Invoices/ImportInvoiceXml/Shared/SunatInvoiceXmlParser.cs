using System.Xml.Linq;

// Parsea una factura electronica UBL 2.1 (formato SUNAT Peru). Pensado para el caso real de
// esta empresa: casi siempre una sola linea con observaciones generales (periodo/fechas),
// a veces varias lineas detalladas. No valida la firma digital ni el CDR, solo extrae datos.
public static class SunatInvoiceXmlParser
{
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

    public static ParsedSunatInvoice Parse(Stream xmlStream)
    {
        // XDocument.Load respeta el encoding declarado en el prolog del XML (ej. ISO-8859-1),
        // siempre que se le pase el stream crudo y no un string ya decodificado con otro encoding.
        var doc = XDocument.Load(xmlStream);
        var root = doc.Root ?? throw new InvalidOperationException("El XML no tiene un elemento raíz válido.");

        var id = (string?)root.Element(Cbc + "ID");
        if (string.IsNullOrWhiteSpace(id) || !id.Contains('-'))
        {
            throw new InvalidOperationException("No se encontró un <cbc:ID> con el formato Serie-Numero esperado.");
        }

        var idParts = id.Split('-', 2);
        var serie = idParts[0];
        var numero = idParts[1];

        var issueDate = (DateTime?)root.Element(Cbc + "IssueDate")
            ?? throw new InvalidOperationException("No se encontró la fecha de emisión (<cbc:IssueDate>).");

        var currencyId = (string?)root.Element(Cbc + "DocumentCurrencyCode") ?? "PEN";
        var invoiceTypeCode = (string?)root.Element(Cbc + "InvoiceTypeCode") ?? "";

        var customerParty = root.Element(Cac + "AccountingCustomerParty")?.Element(Cac + "Party")
            ?? throw new InvalidOperationException("No se encontró la información del cliente (<cac:AccountingCustomerParty>).");

        var customerDocId = (string?)customerParty.Element(Cac + "PartyIdentification")?.Element(Cbc + "ID") ?? "";
        var customerName = (string?)customerParty.Element(Cac + "PartyLegalEntity")?.Element(Cbc + "RegistrationName")
            ?? (string?)customerParty.Element(Cac + "PartyName")?.Element(Cbc + "Name")
            ?? "";

        var paymentTermsBlocks = root.Elements(Cac + "PaymentTerms").ToList();

        // Forma de pago: bloque PaymentTerms[ID=FormaPago] cuyo PaymentMeansID es "Contado" o "Credito".
        var formaPagoBlock = paymentTermsBlocks.FirstOrDefault(pt =>
            (string?)pt.Element(Cbc + "ID") == "FormaPago" &&
            ((string?)pt.Element(Cbc + "PaymentMeansID"))?.Equals("Credito", StringComparison.OrdinalIgnoreCase) == true);

        var typePayment = formaPagoBlock is not null ? "credito" : "contado";

        // Fecha de vencimiento: bloque PaymentTerms[ID=FormaPago] cuyo PaymentMeansID empieza con "Cuota".
        var cuotaBlock = paymentTermsBlocks.FirstOrDefault(pt =>
            (string?)pt.Element(Cbc + "ID") == "FormaPago" &&
            (((string?)pt.Element(Cbc + "PaymentMeansID"))?.StartsWith("Cuota", StringComparison.OrdinalIgnoreCase) ?? false));

        var dueDate = (DateTime?)cuotaBlock?.Element(Cbc + "PaymentDueDate") ?? issueDate;

        // Detraccion: bloque PaymentTerms[ID=Detraccion] (porcentaje/monto) + PaymentMeans[ID=Detraccion] (cuenta).
        var detraccionTermsBlock = paymentTermsBlocks.FirstOrDefault(pt => (string?)pt.Element(Cbc + "ID") == "Detraccion");
        var hasDetraction = detraccionTermsBlock is not null;
        decimal? detractionPercent = hasDetraction ? (decimal?)detraccionTermsBlock!.Element(Cbc + "PaymentPercent") : null;
        decimal? detractionAmount = hasDetraction ? (decimal?)detraccionTermsBlock!.Element(Cbc + "Amount") : null;

        var paymentMeansBlock = root.Element(Cac + "PaymentMeans");
        var detractionAccountNumber = (paymentMeansBlock is not null && (string?)paymentMeansBlock.Element(Cbc + "ID") == "Detraccion")
            ? (string?)paymentMeansBlock.Element(Cac + "PayeeFinancialAccount")?.Element(Cbc + "ID")
            : null;

        var taxTotal = root.Element(Cac + "TaxTotal");
        var igv = (decimal?)taxTotal?.Element(Cbc + "TaxAmount") ?? 0;

        var legalMonetaryTotal = root.Element(Cac + "LegalMonetaryTotal")
            ?? throw new InvalidOperationException("No se encontró <cac:LegalMonetaryTotal> con los montos totales.");

        var subTotal = (decimal?)legalMonetaryTotal.Element(Cbc + "LineExtensionAmount") ?? 0;
        var total = (decimal?)legalMonetaryTotal.Element(Cbc + "PayableAmount") ?? 0;

        var lines = new List<ParsedSunatInvoiceLine>();
        foreach (var lineEl in root.Elements(Cac + "InvoiceLine"))
        {
            var lineId = (string?)lineEl.Element(Cbc + "ID") ?? (lines.Count + 1).ToString();
            var quantity = (decimal?)lineEl.Element(Cbc + "InvoicedQuantity") ?? 0;
            var lineTotal = (decimal?)lineEl.Element(Cbc + "LineExtensionAmount") ?? 0;

            var itemEl = lineEl.Element(Cac + "Item");
            var description = (string?)itemEl?.Element(Cbc + "Description") ?? "";
            var sellerItemCode = (string?)itemEl?.Element(Cac + "SellersItemIdentification")?.Element(Cbc + "ID");

            var unitPrice = (decimal?)lineEl.Element(Cac + "Price")?.Element(Cbc + "PriceAmount")
                ?? (quantity != 0 ? lineTotal / quantity : lineTotal);

            lines.Add(new ParsedSunatInvoiceLine(lineId, description, quantity, unitPrice, lineTotal, sellerItemCode));
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("El XML no tiene ninguna línea de detalle (<cac:InvoiceLine>).");
        }

        return new ParsedSunatInvoice(
            serie, numero, issueDate.Date, currencyId, invoiceTypeCode,
            customerDocId, customerName,
            typePayment, dueDate.Date,
            subTotal, igv, total,
            hasDetraction, detractionPercent, detractionAmount, detractionAccountNumber,
            lines
        );
    }
}

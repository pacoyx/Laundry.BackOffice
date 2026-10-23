// Resultado crudo de parsear el XML SUNAT (UBL 2.1), antes de resolver contra la BD
// (cliente, tipo de documento, cuenta de detraccion). TypePayment ya normalizado a
// "contado"/"credito" (en minuscula), tal como lo exige CreateInvoiceValidator.
public record ParsedSunatInvoiceLine(
    string LineId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total,
    string? SellerItemCode
);

public record ParsedSunatInvoice(
    string Serie,
    string Numero,
    DateTime IssueDate,
    string CurrencyId,
    string InvoiceTypeCode,
    string CustomerDocId,
    string CustomerName,
    string TypePayment,
    DateTime DueDate,
    decimal SubTotal,
    decimal Igv,
    decimal Total,
    bool HasDetraction,
    decimal? DetractionPercent,
    decimal? DetractionAmount,
    string? DetractionAccountNumber,
    List<ParsedSunatInvoiceLine> Lines
);

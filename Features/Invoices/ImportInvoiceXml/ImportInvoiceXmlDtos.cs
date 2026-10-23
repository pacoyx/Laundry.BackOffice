public record ImportInvoiceXmlLineResponse(
    int LineNumber,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total,
    string? SellerItemCode
);

// Todo lo que el parser pudo resolver del XML + validaciones contra la BD (cliente, tipo de
// documento, cuenta de detraccion), listo para que el frontend muestre la vista previa y
// pida al usuario el ProductId de cada linea antes de confirmar.
public record ImportInvoiceXmlPreviewResponse(
    int CustomerId,
    string CustomerName,
    int DocumentTypeId,
    string Serie,
    string Numero,
    DateTime IssueDate,
    DateTime DueDate,
    string CurrencyId,
    string TypePayment,
    int DaysCredit,
    decimal SubTotal,
    decimal Igv,
    decimal Total,
    bool HasDetraction,
    decimal? DetractionPercent,
    decimal? DetractionAmount,
    bool DetractionAccountMatches,
    string? DetractionWarning,
    List<ImportInvoiceXmlLineResponse> Details
);

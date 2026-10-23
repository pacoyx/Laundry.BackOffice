// Recibe de vuelta lo mismo que devolvio el preview (cabecera + montos del XML, sin
// recalcular), mas BranchId/PaymentMethod (el XML no los trae, los elige el usuario) y el
// ProductId ya asignado a cada linea.
public record ConfirmImportInvoiceXmlRequest(
    int CustomerId,
    int BranchId,
    int DocumentTypeId,
    string Serie,
    string Numero,
    string CurrencyId,
    decimal ExchangeRate,
    DateTime IssueDate,
    DateTime DueDate,
    string PaymentMethod,
    string TypePayment,
    int DaysCredit,
    decimal SubTotal,
    decimal Igv,
    decimal Total,
    bool HasDetraction,
    decimal? DetractionPercent,
    decimal? DetractionAmount,
    string? Notes,
    List<ConfirmImportInvoiceXmlLineRequest> Details
);

public record ConfirmImportInvoiceXmlLineRequest(
    int ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total
);

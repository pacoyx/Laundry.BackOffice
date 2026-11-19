public record RegisterPurchasePaymentRequestDto(
    int PaymentMethodId,
    decimal Amount,
    string? OperationNumber,
    string? Observation,
    DateTime? PaymentDate = null
);

public record RegisterPurchasePaymentResponseDto(
    long Id,
    int PurchaseInvoiceId,
    int PaymentMethodId,
    string PaymentMethodName,
    DateTime PaymentDate,
    decimal Amount,
    string? OperationNumber,
    string? Observation
);

public class RegisterPurchasePaymentResult
{
    public bool NotFound { get; set; }
    public string? Error { get; set; }
    public RegisterPurchasePaymentResponseDto? Data { get; set; }
}

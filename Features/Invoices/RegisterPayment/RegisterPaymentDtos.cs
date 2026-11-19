public record RegisterPaymentRequest(
    int PaymentMethodId,
    decimal Amount,
    string? OperationNumber,
    string? Observation,
    DateTime? PaymentDate = null
);

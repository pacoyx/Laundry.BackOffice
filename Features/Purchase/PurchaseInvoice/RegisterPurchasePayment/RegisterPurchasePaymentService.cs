using Microsoft.EntityFrameworkCore;

public class RegisterPurchasePaymentService : IRegisterPurchasePaymentService
{
    private readonly RecepcionDbContext _context;

    public RegisterPurchasePaymentService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterPurchasePaymentResult> RegisterPaymentAsync(int purchaseInvoiceId, RegisterPurchasePaymentRequestDto request)
    {
        var purchaseInvoice = await _context.PurchaseInvoices.FirstOrDefaultAsync(x => x.Id == purchaseInvoiceId);
        if (purchaseInvoice == null)
        {
            return new RegisterPurchasePaymentResult { NotFound = true, Error = "Compra no encontrada." };
        }

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.IsActive);
        if (paymentMethod == null)
        {
            return new RegisterPurchasePaymentResult { Error = "Método de pago inválido o inactivo." };
        }

        var error = PurchaseInvoicePaymentRules.Validate(purchaseInvoice, request.Amount);
        if (error is not null)
        {
            return new RegisterPurchasePaymentResult { Error = error };
        }

        var payment = new PurchaseInvoicePayment
        {
            PurchaseInvoiceId = purchaseInvoice.Id,
            PaymentMethodId = request.PaymentMethodId,
            PaymentDate = request.PaymentDate is { } paymentDate
                ? (paymentDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(paymentDate, DateTimeKind.Utc)
                    : paymentDate.ToUniversalTime())
                : DateTime.UtcNow,
            Amount = request.Amount,
            OperationNumber = request.OperationNumber,
            Observation = request.Observation
        };

        PurchaseInvoicePaymentRules.Apply(purchaseInvoice, request.Amount);

        await _context.PurchaseInvoicePayments.AddAsync(payment);
        await _context.SaveChangesAsync();

        var response = new RegisterPurchasePaymentResponseDto(
            payment.Id, payment.PurchaseInvoiceId, payment.PaymentMethodId, paymentMethod.Name,
            payment.PaymentDate, payment.Amount, payment.OperationNumber, payment.Observation);

        return new RegisterPurchasePaymentResult { Data = response };
    }
}

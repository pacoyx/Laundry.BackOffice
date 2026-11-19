using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class RegisterPaymentEndpoint
{
    public static void MapRegisterPayment(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/payments", async (
            long id,
            RegisterPaymentRequest request,
            IValidator<RegisterPaymentRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id);
            if (invoice is null)
            {
                return Results.NotFound("Factura no encontrada.");
            }

            var paymentMethod = await db.PaymentMethods
                .FirstOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.IsActive);
            if (paymentMethod is null)
            {
                return Results.BadRequest("Método de pago inválido o inactivo.");
            }

            var error = InvoicePaymentRules.Validate(invoice, request.Amount);
            if (error is not null)
            {
                return Results.BadRequest(error);
            }

            var payment = new InvoicePayment
            {
                InvoiceId = invoice.Id,
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

            InvoicePaymentRules.Apply(invoice, request.Amount);

            await db.InvoicePayments.AddAsync(payment);
            await db.SaveChangesAsync();

            var response = new InvoicePaymentResponse(
                payment.Id, payment.InvoiceId, payment.PaymentMethodId, paymentMethod.Name,
                payment.PaymentDate, payment.Amount, payment.OperationNumber, payment.Observation,
                payment.CollectionSheetId);

            return Results.Ok(new ApiResponse<InvoicePaymentResponse>
            {
                Success = true,
                Message = "Pago registrado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .RequireAuthorization();
    }
}

using FluentValidation;

public class SendTestInvoiceReminderValidator : AbstractValidator<SendTestInvoiceReminderRequest>
{
    public SendTestInvoiceReminderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.TestEmail).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

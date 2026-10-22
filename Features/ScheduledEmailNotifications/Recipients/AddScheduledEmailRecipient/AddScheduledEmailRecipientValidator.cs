using FluentValidation;

public class AddScheduledEmailRecipientValidator : AbstractValidator<AddScheduledEmailRecipientRequest>
{
    public AddScheduledEmailRecipientValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.RecipientType).IsInEnum();
    }
}

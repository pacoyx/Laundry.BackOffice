using FluentValidation;

public class CreateBankValidator : AbstractValidator<CreateBankRequest>
{
    public CreateBankValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShortName).MaximumLength(20);
    }
}

using FluentValidation;

public class UpdateBankValidator : AbstractValidator<UpdateBankRequest>
{
    public UpdateBankValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShortName).MaximumLength(20);
    }
}

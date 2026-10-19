using FluentValidation;

public class UpdateBankAccountTypeValidator : AbstractValidator<UpdateBankAccountTypeRequest>
{
    public UpdateBankAccountTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

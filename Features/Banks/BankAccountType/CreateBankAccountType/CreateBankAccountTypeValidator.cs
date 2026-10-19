using FluentValidation;

public class CreateBankAccountTypeValidator : AbstractValidator<CreateBankAccountTypeRequest>
{
    public CreateBankAccountTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

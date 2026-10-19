using FluentValidation;

public class CreateCompanyBankAccountValidator : AbstractValidator<CreateCompanyBankAccountRequest>
{
    public CreateCompanyBankAccountValidator()
    {
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.BankAccountTypeId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).NotEmpty().Length(3);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.InterbankAccountNumber).MaximumLength(30);
        RuleFor(x => x.Description).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

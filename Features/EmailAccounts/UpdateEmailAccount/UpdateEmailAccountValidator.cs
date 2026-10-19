using FluentValidation;

public class UpdateEmailAccountValidator : AbstractValidator<UpdateEmailAccountRequest>
{
    public UpdateEmailAccountValidator()
    {
        RuleFor(x => x.Operacion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Host).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Port).InclusiveBetween(1, 65535);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(200).EmailAddress();
    }
}

using FluentValidation;

public class SendAdvisoryContactValidator : AbstractValidator<SendAdvisoryContactRequest>
{
    public SendAdvisoryContactValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.TipoInteres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mensaje).NotEmpty().MaximumLength(2000);
    }
}

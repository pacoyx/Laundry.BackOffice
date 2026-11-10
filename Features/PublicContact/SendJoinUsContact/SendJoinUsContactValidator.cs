using FluentValidation;

public class SendJoinUsContactValidator : AbstractValidator<SendJoinUsContactRequest>
{
    public SendJoinUsContactValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.AreaInteres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mensaje).MaximumLength(2000);
    }
}

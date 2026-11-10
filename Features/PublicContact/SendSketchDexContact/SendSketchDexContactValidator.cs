using FluentValidation;

public class SendSketchDexContactValidator : AbstractValidator<SendSketchDexContactRequest>
{
    public SendSketchDexContactValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NombreNegocio).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.CantidadUsuarios).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Mensaje).MaximumLength(2000);
    }
}

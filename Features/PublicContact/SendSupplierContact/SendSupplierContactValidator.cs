using FluentValidation;

public class SendSupplierContactValidator : AbstractValidator<SendSupplierContactRequest>
{
    public SendSupplierContactValidator()
    {
        RuleFor(x => x.NombreComercial).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RazonSocial).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactoNombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Telefono).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Mensaje).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Categorias).Must(c => c == null || c.Count <= 10)
            .WithMessage("Demasiadas categorías seleccionadas.");
    }
}

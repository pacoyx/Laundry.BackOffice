using FluentValidation;

public class CreateGuiaRemisionValidator : AbstractValidator<CreateGuiaRemisionRequest>
{
    public CreateGuiaRemisionValidator()
    {
        RuleFor(x => x.Serie).NotEmpty().MaximumLength(5);
        RuleFor(x => x.Numero).NotEmpty().MaximumLength(10);
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverDni).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DeliveryAddress).MaximumLength(300);
        RuleFor(x => x.FechaTraslado).NotEqual(default(DateTime));
        RuleFor(x => x.MotivoTraslado).MaximumLength(100);
        RuleFor(x => x.OriginAddress).MaximumLength(300);
    }
}

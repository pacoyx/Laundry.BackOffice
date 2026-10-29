using FluentValidation;

public class CreateZonePriceListValidator : AbstractValidator<CreateZonePriceListRequest>
{
    public CreateZonePriceListValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");

        RuleFor(x => x.Details).NotEmpty()
            .WithMessage("La tarifa debe tener al menos un servicio con precio.");

        RuleFor(x => x.Details)
            .Must(details => details.Select(d => d.ServiceItemId).Distinct().Count() == details.Count)
            .When(x => x.Details is { Count: > 0 })
            .WithMessage("No se puede repetir el mismo servicio en la tarifa.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ServiceItemId).GreaterThan(0);
            detail.RuleFor(d => d.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateZonePriceListValidator : AbstractValidator<UpdateZonePriceListRequest>
{
    public UpdateZonePriceListValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");

        RuleFor(x => x.Details).NotEmpty()
            .WithMessage("La tarifa debe tener al menos un servicio con precio.");

        RuleFor(x => x.Details)
            .Must(details => details.Select(d => d.ServiceItemId).Distinct().Count() == details.Count)
            .When(x => x.Details is { Count: > 0 })
            .WithMessage("No se puede repetir el mismo servicio en la tarifa.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ServiceItemId).GreaterThan(0);
            detail.RuleFor(d => d.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

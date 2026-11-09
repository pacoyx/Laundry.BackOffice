using FluentValidation;

public class CreateGarmentIntakeTicketValidator : AbstractValidator<CreateGarmentIntakeTicketRequest>
{
    public CreateGarmentIntakeTicketValidator()
    {
        RuleFor(x => x.Details).NotEmpty().WithMessage("Debe registrar al menos una línea.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.WorkerId).GreaterThan(0);
            detail.RuleFor(d => d.ServiceItemId).GreaterThan(0);
            detail.RuleFor(d => d.Quantity).GreaterThan(0);
            detail.RuleFor(d => d.Observations).MaximumLength(500);
        });
    }
}

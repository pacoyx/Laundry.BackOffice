using FluentValidation;

public class CreatePickupGuideValidator : AbstractValidator<CreatePickupGuideRequest>
{
    public CreatePickupGuideValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.BranchSalesId).GreaterThan(0);
        RuleFor(x => x.Observation).MaximumLength(500);
        RuleFor(x => x.PhysicalGuideNumber).MaximumLength(50);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId.HasValue);
        RuleFor(x => x.VehicleId).GreaterThan(0).When(x => x.VehicleId.HasValue);
        RuleFor(x => x.Details).NotEmpty()
            .WithMessage("La guía debe tener al menos un detalle.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ServiceItemId).GreaterThan(0);
            detail.RuleFor(d => d.Quantity).GreaterThanOrEqualTo(0);
            detail.RuleFor(d => d.Weight).GreaterThanOrEqualTo(0).When(d => d.Weight.HasValue);
            detail.RuleFor(d => d.Observation).MaximumLength(500);
        });
    }
}

using FluentValidation;

public class CreatePickupGuideRevisionValidator : AbstractValidator<CreatePickupGuideRevisionRequest>
{
    public CreatePickupGuideRevisionValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.Observation).MaximumLength(500);
        RuleFor(x => x.Details).NotEmpty()
            .WithMessage("La revisión debe incluir al menos un detalle corregido.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.Reason).NotEmpty().MaximumLength(500);

            detail.When(d => d.ActionType == PickupGuideRevisionActionType.QuantityAdjustment, () =>
            {
                detail.RuleFor(d => d.PickupGuideDetailId).NotNull();
                detail.RuleFor(d => d.PickupGuideDetailId!.Value).GreaterThan(0)
                    .When(d => d.PickupGuideDetailId.HasValue);
                detail.RuleFor(d => d.NewQuantity).NotNull();
                detail.RuleFor(d => d.NewQuantity!.Value).GreaterThanOrEqualTo(0)
                    .When(d => d.NewQuantity.HasValue);
            });

            detail.When(d => d.ActionType == PickupGuideRevisionActionType.AddedItem, () =>
            {
                detail.RuleFor(d => d.ServiceItemId).NotNull();
                detail.RuleFor(d => d.ServiceItemId!.Value).GreaterThan(0)
                    .When(d => d.ServiceItemId.HasValue);
                detail.RuleFor(d => d.NewQuantity).NotNull();
                detail.RuleFor(d => d.NewQuantity!.Value).GreaterThan(0)
                    .When(d => d.NewQuantity.HasValue);
            });

            detail.When(d => d.ActionType == PickupGuideRevisionActionType.RemovedItem, () =>
            {
                detail.RuleFor(d => d.PickupGuideDetailId).NotNull();
                detail.RuleFor(d => d.PickupGuideDetailId!.Value).GreaterThan(0)
                    .When(d => d.PickupGuideDetailId.HasValue);
            });
        });
    }
}

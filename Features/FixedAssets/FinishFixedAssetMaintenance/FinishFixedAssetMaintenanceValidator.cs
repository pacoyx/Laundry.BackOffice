using FluentValidation;

public class FinishFixedAssetMaintenanceValidator : AbstractValidator<FinishFixedAssetMaintenanceRequest>
{
    public FinishFixedAssetMaintenanceValidator()
    {
        RuleFor(x => x.ActualEndDate).NotEqual(default(DateTime))
            .WithMessage("Debes indicar la fecha de fin del mantenimiento.");
        RuleFor(x => x.FinalCost).GreaterThanOrEqualTo(0).When(x => x.FinalCost.HasValue);
        RuleFor(x => x.ClosingNotes).MaximumLength(500);
    }
}

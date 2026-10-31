using FluentValidation;

public class CreateFixedAssetMaintenanceValidator : AbstractValidator<CreateFixedAssetMaintenanceRequest>
{
    public CreateFixedAssetMaintenanceValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateTime));
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Observations).MaximumLength(500);
    }
}

using FluentValidation;

public class UpdateFixedAssetValidator : AbstractValidator<UpdateFixedAssetRequest>
{
    public UpdateFixedAssetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.AcquisitionDate).NotEqual(default(DateTime));
        RuleFor(x => x.AcquisitionCost).GreaterThan(0);
        RuleFor(x => x.UsefulLifeYearsOverride).GreaterThan(0).When(x => x.UsefulLifeYearsOverride.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

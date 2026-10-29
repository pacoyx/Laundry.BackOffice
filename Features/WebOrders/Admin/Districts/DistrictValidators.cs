using FluentValidation;

public class CreateDistrictValidator : AbstractValidator<CreateDistrictRequest>
{
    public CreateDistrictValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZoneId).GreaterThan(0);
    }
}

public class UpdateDistrictValidator : AbstractValidator<UpdateDistrictRequest>
{
    public UpdateDistrictValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZoneId).GreaterThan(0);
    }
}

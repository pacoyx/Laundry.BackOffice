using FluentValidation;

public class UpdateCompanyWorkerValidator : AbstractValidator<UpdateCompanyWorkerRequest>
{
    public UpdateCompanyWorkerValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Area).NotEmpty().MaximumLength(100);
    }
}

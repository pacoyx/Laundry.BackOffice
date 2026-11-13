using FluentValidation;

public class CreateFixedAssetMaintenanceValidator : AbstractValidator<CreateFixedAssetMaintenanceRequest>
{
    public CreateFixedAssetMaintenanceValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateTime));
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Observations).MaximumLength(500);
        RuleFor(x => x.ServiceProvider).MaximumLength(150);

        When(x => x.PutAssetOutOfService, () =>
        {
            RuleFor(x => x.OutOfServiceReason)
                .NotEmpty().WithMessage("Debes indicar el motivo por el que el activo queda en mantenimiento.")
                .MaximumLength(300);
            RuleFor(x => x.EstimatedEndDate)
                .NotNull().WithMessage("Debes indicar la fecha fin estimada del mantenimiento.");
            RuleFor(x => x.EstimatedEndDate)
                .Must((req, end) => end!.Value.Date >= req.Date.Date)
                .When(x => x.EstimatedEndDate.HasValue)
                .WithMessage("La fecha fin estimada no puede ser anterior a la fecha de inicio.");
        });
    }
}

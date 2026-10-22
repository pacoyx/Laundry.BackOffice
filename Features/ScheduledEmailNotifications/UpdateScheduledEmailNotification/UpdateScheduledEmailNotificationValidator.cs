using FluentValidation;

public class UpdateScheduledEmailNotificationValidator : AbstractValidator<UpdateScheduledEmailNotificationRequest>
{
    public UpdateScheduledEmailNotificationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Hour).InclusiveBetween(0, 23);
        RuleFor(x => x.Minute).InclusiveBetween(0, 59);
        RuleFor(x => x.DaysOfWeek).NotEmpty().WithMessage("Debe seleccionar al menos un día de la semana.");
        RuleForEach(x => x.DaysOfWeek).IsInEnum();
        RuleFor(x => x.SubjectTemplate).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BodyTemplate).NotEmpty();
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverDocument).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SignatureName).MaximumLength(150);
        RuleFor(x => x.SignaturePosition).MaximumLength(100);
        RuleFor(x => x.SignaturePhone).MaximumLength(30);
    }
}

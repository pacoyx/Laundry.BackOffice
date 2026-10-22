using FluentValidation;

public class SendTestScheduledEmailNotificationValidator : AbstractValidator<SendTestScheduledEmailNotificationRequest>
{
    public SendTestScheduledEmailNotificationValidator()
    {
        RuleFor(x => x.TestEmail).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

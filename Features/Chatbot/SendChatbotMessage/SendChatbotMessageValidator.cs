using FluentValidation;

public class ChatbotMessageValidator : AbstractValidator<ChatbotMessageRequest>
{
    public ChatbotMessageValidator()
    {
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.ExternalReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

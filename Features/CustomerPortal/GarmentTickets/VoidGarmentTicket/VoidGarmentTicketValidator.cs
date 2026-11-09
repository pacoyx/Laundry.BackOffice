using FluentValidation;

public class VoidGarmentIntakeTicketValidator : AbstractValidator<VoidGarmentIntakeTicketRequest>
{
    public VoidGarmentIntakeTicketValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

using FluentValidation;

public class ResetCustomerUserPasswordAdminValidator : AbstractValidator<ResetCustomerUserPasswordAdminRequest>
{
    public ResetCustomerUserPasswordAdminValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

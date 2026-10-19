using FluentValidation;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileRequestDto>
{
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

public class ChangeMyPasswordValidator : AbstractValidator<ChangeMyPasswordRequestDto>
{
    public ChangeMyPasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("La nueva contraseña debe ser diferente a la actual.");
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("La confirmación de contraseña no coincide.");
    }
}

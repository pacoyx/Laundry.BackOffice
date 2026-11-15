using FluentValidation;

public class RegisterLoanValidator : AbstractValidator<RegisterLoanRequestDto>
{
    public const int MaxInstallments = 48;

    public RegisterLoanValidator()
    {
        RuleFor(x => x.LoanDate).NotEmpty().WithMessage("La fecha es obligatoria.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");
        RuleFor(x => x.Observation).MaximumLength(500);
        RuleFor(x => x.InstallmentCount)
            .InclusiveBetween(1, MaxInstallments)
            .WithMessage($"El número de cuotas debe estar entre 1 y {MaxInstallments}.");
        RuleFor(x => x)
            .Must(x => x.InstallmentCount <= 0 || decimal.Round(x.Amount / x.InstallmentCount, 2) >= 0.01m)
            .WithMessage("El monto es demasiado pequeño para dividirlo en esa cantidad de cuotas.");
    }
}

using FluentValidation;

public class CreatePayrollPeriodValidator : AbstractValidator<CreatePayrollPeriodRequestDto>
{
    public CreatePayrollPeriodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del periodo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del periodo no puede superar los 100 caracteres.");
        RuleFor(x => x.Frequency).IsInEnum().WithMessage("La frecuencia no es válida.");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha de fin no puede ser anterior a la fecha de inicio.");
    }
}

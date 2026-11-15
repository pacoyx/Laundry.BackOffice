using FluentValidation;

public class CreatePayrollDiscountValidator : AbstractValidator<CreatePayrollDiscountRequestDto>
{
    public CreatePayrollDiscountValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Seleccione un trabajador.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.")
            .PrecisionScale(18, 2, true).WithMessage("El monto admite como máximo 2 decimales.");
    }
}

using FluentValidation;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequestDto>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(100).WithMessage("Los nombres no pueden superar los 100 caracteres.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(100).WithMessage("Los apellidos no pueden superar los 100 caracteres.");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres.");
        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("La dirección no puede superar los 200 caracteres.");
        RuleFor(x => x.AreaId).GreaterThan(0).WithMessage("Seleccione un área.");
        RuleFor(x => x.PositionId).GreaterThan(0).WithMessage("Seleccione un cargo.");
        RuleFor(x => x.ContractType).IsInEnum().WithMessage("El tipo de contrato no es válido.");
        RuleFor(x => x.BaseSalary).GreaterThanOrEqualTo(0).WithMessage("El sueldo base no puede ser negativo.");
    }
}

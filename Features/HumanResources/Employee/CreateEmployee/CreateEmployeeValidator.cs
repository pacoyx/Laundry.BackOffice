using FluentValidation;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequestDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(20).WithMessage("El código no puede superar los 20 caracteres.");
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(20).WithMessage("El número de documento no puede superar los 20 caracteres.");
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
        RuleFor(x => x.HireDate).GreaterThanOrEqualTo(x => x.BirthDate)
            .WithMessage("La fecha de contratación no puede ser anterior a la fecha de nacimiento.");

        RuleForEach(x => x.EmergencyContacts).ChildRules(contact =>
        {
            contact.RuleFor(c => c.Name)
                .NotEmpty().WithMessage("El nombre del contacto de emergencia es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre del contacto de emergencia no puede superar los 150 caracteres.");
            contact.RuleFor(c => c.Relationship)
                .NotEmpty().WithMessage("El parentesco del contacto de emergencia es obligatorio.")
                .MaximumLength(50).WithMessage("El parentesco no puede superar los 50 caracteres.");
            contact.RuleFor(c => c.PhoneNumber)
                .NotEmpty().WithMessage("El teléfono del contacto de emergencia es obligatorio.")
                .MaximumLength(20).WithMessage("El teléfono del contacto de emergencia no puede superar los 20 caracteres.");
        });

        RuleForEach(x => x.BankAccounts).ChildRules(account =>
        {
            account.RuleFor(a => a.BankName)
                .NotEmpty().WithMessage("El banco de la cuenta es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre del banco no puede superar los 100 caracteres.");
            account.RuleFor(a => a.AccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es obligatorio.")
                .MaximumLength(30).WithMessage("El número de cuenta no puede superar los 30 caracteres.");
        });
    }
}

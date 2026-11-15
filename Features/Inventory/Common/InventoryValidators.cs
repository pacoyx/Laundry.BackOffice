using FluentValidation;

public class SaveWarehouseValidator : AbstractValidator<SaveWarehouseRequest>
{
    public SaveWarehouseValidator()
    {
        RuleFor(x => x.BranchSalesId).GreaterThan(0).WithMessage("Seleccione una sucursal.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(20).WithMessage("El código no puede superar los 20 caracteres.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}

public class CreateInventoryMovementValidator : AbstractValidator<CreateInventoryMovementRequest>
{
    public CreateInventoryMovementValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("El tipo de movimiento no es válido.");
        RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Seleccione un almacén.");
        RuleFor(x => x.Reason).MaximumLength(300).WithMessage("El motivo no puede superar los 300 caracteres.");

        RuleFor(x => x.TargetWarehouseId)
            .NotNull().WithMessage("Seleccione el almacén destino.")
            .When(x => x.Type == InventoryMovementType.Transfer);
        RuleFor(x => x.AreaId)
            .NotNull().WithMessage("Seleccione el área que consume.")
            .When(x => x.Type == InventoryMovementType.Consumption);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Indique el motivo del ajuste.")
            .When(x => x.Type is InventoryMovementType.AdjustmentIn or InventoryMovementType.AdjustmentOut);

        RuleFor(x => x.Lines).NotEmpty().WithMessage("Agregue al menos un producto.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0).WithMessage("Hay una línea sin producto.");
            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Las cantidades deben ser mayores a cero.")
                .PrecisionScale(18, 4, true).WithMessage("La cantidad admite como máximo 4 decimales.");
            line.RuleFor(l => l.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("El costo unitario no puede ser negativo.");
        });
        RuleForEach(x => x.Lines)
            .Must(l => l.UnitCost.HasValue)
            .WithMessage("El inventario inicial requiere el costo unitario de cada producto.")
            .When(x => x.Type == InventoryMovementType.InitialBalance);
        RuleFor(x => x.Lines)
            .Must(lines => lines.Select(l => l.ProductId).Distinct().Count() == lines.Count)
            .WithMessage("Hay productos repetidos: agrúpelos en una sola línea.");
    }
}

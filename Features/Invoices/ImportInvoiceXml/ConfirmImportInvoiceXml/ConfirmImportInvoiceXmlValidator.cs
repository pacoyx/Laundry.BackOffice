using FluentValidation;

public class ConfirmImportInvoiceXmlValidator : AbstractValidator<ConfirmImportInvoiceXmlRequest>
{
    public ConfirmImportInvoiceXmlValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.DocumentTypeId).GreaterThan(0);
        RuleFor(x => x.Serie).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Numero).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CurrencyId).NotEmpty().MaximumLength(3);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TypePayment).NotEmpty().Must(x => x == "contado" || x == "credito")
            .WithMessage("El tipo de pago debe ser 'contado' o 'credito'.");
        RuleFor(x => x.DaysCredit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.DetractionPercent).GreaterThanOrEqualTo(0).When(x => x.HasDetraction);
        RuleFor(x => x.DetractionAmount).GreaterThanOrEqualTo(0).When(x => x.HasDetraction);

        RuleFor(x => x.Details).NotEmpty()
            .WithMessage("La factura debe tener al menos un detalle.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId).GreaterThan(0)
                .WithMessage("Debe asignar un producto/servicio del catálogo a cada línea del XML.");
            detail.RuleFor(d => d.Description).NotEmpty().MaximumLength(500);
            detail.RuleFor(d => d.Quantity).GreaterThan(0);
            detail.RuleFor(d => d.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

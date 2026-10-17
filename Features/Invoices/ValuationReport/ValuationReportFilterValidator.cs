using FluentValidation;

public class ValuationReportFilterValidator : AbstractValidator<ValuationReportFilter>
{
    public ValuationReportFilterValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("El cliente es requerido.");

        RuleFor(x => x.FromDate)
            .NotEqual(default(DateTime))
            .WithMessage("La fecha 'Desde' es requerida.");

        RuleFor(x => x.ToDate)
            .NotEqual(default(DateTime))
            .WithMessage("La fecha 'Hasta' es requerida.");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("La fecha 'Hasta' no puede ser anterior a la fecha 'Desde'.");
    }
}

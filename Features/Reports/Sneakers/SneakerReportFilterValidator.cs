using FluentValidation;

public class SneakerReportFilterValidator : AbstractValidator<SneakerReportFilter>
{
    public SneakerReportFilterValidator()
    {
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom!.Value)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("La fecha 'Hasta' no puede ser anterior a la fecha 'Desde'.");

        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue)
            .WithMessage("La sucursal indicada no es válida.");

        RuleFor(x => x.BrandId).GreaterThan(0).When(x => x.BrandId.HasValue)
            .WithMessage("La marca indicada no es válida.");

        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId.HasValue)
            .WithMessage("El cliente indicado no es válido.");

        RuleFor(x => x.TicketStatus).MaximumLength(1);
    }
}

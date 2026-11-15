using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public interface IPayrollExportService
{
    Task<PayrollExportResult> GenerateExcelAsync(long payrollPeriodId);
    Task<PayrollExportResult> GeneratePdfAsync(long payrollPeriodId);
}

public class PayrollExportService : IPayrollExportService
{
    private readonly RecepcionDbContext _context;
    private readonly ICompanyProfileService _companyProfileService;

    public PayrollExportService(RecepcionDbContext context, ICompanyProfileService companyProfileService)
    {
        _context = context;
        _companyProfileService = companyProfileService;
    }

    public async Task<PayrollExportResult> GenerateExcelAsync(long payrollPeriodId)
    {
        var (status, data) = await BuildDataAsync(payrollPeriodId);
        if (data == null) return new PayrollExportResult(status);

        var bytes = PayrollExcelBuilder.Build(data);
        return new PayrollExportResult(PayrollExportStatus.Ok, bytes, $"Planilla_{FileSafe(data.PeriodName)}.xlsx");
    }

    public async Task<PayrollExportResult> GeneratePdfAsync(long payrollPeriodId)
    {
        var (status, data) = await BuildDataAsync(payrollPeriodId);
        if (data == null) return new PayrollExportResult(status);

        var logo = await _companyProfileService.GetCompanyLogoBytesAsync();
        var bytes = new PayrollPdfDocument(data, logo).GeneratePdf();
        return new PayrollExportResult(PayrollExportStatus.Ok, bytes, $"Planilla_{FileSafe(data.PeriodName)}.pdf");
    }

    private async Task<(PayrollExportStatus, PayrollExportData?)> BuildDataAsync(long payrollPeriodId)
    {
        var period = await _context.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null) return (PayrollExportStatus.PeriodNotFound, null);

        var payrolls = await _context.Payrolls
            .AsNoTracking()
            .Include(x => x.Employee!).ThenInclude(e => e.Area)
            .Include(x => x.Employee!).ThenInclude(e => e.Position)
            .Include(x => x.PayrollDetails)
            .Where(x => x.PayrollPeriodId == payrollPeriodId)
            .OrderBy(x => x.Employee!.LastName).ThenBy(x => x.Employee!.FirstName)
            .ToListAsync();

        if (payrolls.Count == 0) return (PayrollExportStatus.NoPayrolls, null);

        var company = await _companyProfileService.GetCompanyAsync();

        var data = new PayrollExportData
        {
            CompanyName = company?.NameCompany ?? string.Empty,
            CompanyDocument = company == null ? string.Empty : $"{company.DocumentType} {company.NumberType}".Trim(),
            PeriodName = period.Name,
            FrequencyLabel = PayrollExportLabels.Frequency(period.Frequency),
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsClosed = period.IsClosed,
            Rows = payrolls.Select(ToRow).ToList()
        };

        return (PayrollExportStatus.Ok, data);
    }

    private static PayrollExportRow ToRow(Payroll payroll)
    {
        var details = payroll.PayrollDetails ?? new List<PayrollDetail>();
        decimal Sum(PayrollConceptType type) => details.Where(d => d.ConceptType == type).Sum(d => d.Amount);
        var employee = payroll.Employee!;

        return new PayrollExportRow
        {
            EmployeeCode = employee.Code,
            DocumentNumber = employee.DocumentNumber,
            EmployeeName = $"{employee.LastName}, {employee.FirstName}",
            AreaName = employee.Area?.Name ?? string.Empty,
            PositionName = employee.Position?.Name ?? string.Empty,
            MonthlySalary = employee.BaseSalary,
            BaseAmount = Sum(PayrollConceptType.BaseSalary),
            ExtraHours = Sum(PayrollConceptType.ExtraHours),
            Bonus = Sum(PayrollConceptType.Bonus),
            GrossAmount = payroll.GrossAmount,
            AbsenceDiscount = -Sum(PayrollConceptType.AbsenceDiscount),
            LateArrivalDiscount = -Sum(PayrollConceptType.LateArrivalDiscount),
            LoanDiscount = -Sum(PayrollConceptType.LoanDiscount),
            AdvanceDiscount = -Sum(PayrollConceptType.AdvanceDiscount),
            OtherDiscount = -Sum(PayrollConceptType.OtherDiscount),
            TotalDiscounts = payroll.Discounts,
            NetAmount = payroll.NetAmount,
            Lines = details
                .OrderBy(d => d.ConceptType).ThenBy(d => d.Id)
                .Select(d => new PayrollExportDetailLine
                {
                    ConceptType = d.ConceptType,
                    ConceptLabel = PayrollExportLabels.Concept(d.ConceptType),
                    Description = d.Description,
                    Amount = d.Amount
                }).ToList()
        };
    }

    private static string FileSafe(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalid.Contains(c) || c == ' ' ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "Periodo" : cleaned;
    }
}

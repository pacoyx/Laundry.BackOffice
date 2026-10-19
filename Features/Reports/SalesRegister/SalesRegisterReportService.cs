using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class SalesRegisterReportService : ISalesRegisterReportService
{
    private readonly RecepcionDbContext _db;
    private readonly ICompanyProfileService _companyProfileService;

    public SalesRegisterReportService(RecepcionDbContext db, ICompanyProfileService companyProfileService)
    {
        _db = db;
        _companyProfileService = companyProfileService;
    }

    public async Task<List<SalesRegisterRowDto>> GetDataAsync(SalesRegisterFilter filter)
    {
        return await BuildQuery(filter).ToListAsync();
    }

    public async Task<byte[]> GeneratePdfAsync(SalesRegisterFilter filter)
    {
        var rows = await GetDataAsync(filter);
        var totals = CalculateTotals(rows);
        var (branchName, customerName) = await ResolveFilterNamesAsync(filter);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync();

        var document = new SalesRegisterPdfDocument(rows, totals, filter, branchName, customerName, logoImage);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateExcelAsync(SalesRegisterFilter filter)
    {
        var rows = await GetDataAsync(filter);
        var totals = CalculateTotals(rows);

        return SalesRegisterExcelBuilder.Build(rows, totals);
    }

    private IQueryable<SalesRegisterRowDto> BuildQuery(SalesRegisterFilter filter)
    {
        var fromDate = filter.FromDate.Date;
        var toDate = filter.ToDate.Date;

        var query = _db.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.IssueDate.Date >= fromDate && x.IssueDate.Date <= toDate)
            .AsQueryable();

        if (filter.BranchId.HasValue)
        {
            query = query.Where(x => x.BranchId == filter.BranchId.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == filter.CustomerId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        return query
            .OrderBy(x => x.IssueDate)
            .ThenBy(x => x.Serie)
            .ThenBy(x => x.Numero)
            .Select(x => new SalesRegisterRowDto
            {
                InvoiceId = x.Id,
                IssueDate = x.IssueDate,
                CustomerName = x.Customer.CustomerType == CustomerType.Company && x.Customer.BillingProfile != null
                    ? x.Customer.BillingProfile.BusinessName
                    : (x.Customer.FirtsName + " " + (x.Customer.LastName ?? "")).Trim(),
                SerieNumero = x.Serie + "-" + x.Numero,
                SubTotal = x.SubTotal,
                Igv = x.Igv,
                Total = x.Total,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                AmountPaid = x.AmountPaid,
                Balance = x.Balance,
                PaymentMethod = x.PaymentMethod,
                TypePayment = x.TypePayment,
                DaysCredit = x.DaysCredit,
                Status = x.Status.ToString()
            });
    }

    private async Task<(string? BranchName, string? CustomerName)> ResolveFilterNamesAsync(SalesRegisterFilter filter)
    {
        string? branchName = null;
        string? customerName = null;

        if (filter.BranchId.HasValue)
        {
            branchName = await _db.BranchSales
                .Where(x => x.Id == filter.BranchId.Value)
                .Select(x => x.Description)
                .FirstOrDefaultAsync();
        }

        if (filter.CustomerId.HasValue)
        {
            customerName = await _db.Customers
                .Where(x => x.Id == filter.CustomerId.Value)
                .Select(x => x.CustomerType == CustomerType.Company && x.BillingProfile != null
                    ? x.BillingProfile.BusinessName
                    : (x.FirtsName + " " + (x.LastName ?? "")).Trim())
                .FirstOrDefaultAsync();
        }

        return (branchName, customerName);
    }

    public static SalesRegisterTotalsDto CalculateTotals(List<SalesRegisterRowDto> rows)
    {
        return new SalesRegisterTotalsDto
        {
            CantidadDocumentos = rows.Count,
            TotalVentas = rows.Sum(x => x.Total),
            TotalCobrado = rows.Sum(x => x.AmountPaid),
            TotalPendiente = rows.Sum(x => x.Balance)
        };
    }
}

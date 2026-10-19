using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class PurchaseRegisterReportService : IPurchaseRegisterReportService
{
    private readonly RecepcionDbContext _db;
    private readonly ICompanyProfileService _companyProfileService;

    public PurchaseRegisterReportService(RecepcionDbContext db, ICompanyProfileService companyProfileService)
    {
        _db = db;
        _companyProfileService = companyProfileService;
    }

    public async Task<List<PurchaseRegisterRowDto>> GetDataAsync(PurchaseRegisterFilter filter)
    {
        return await BuildQuery(filter).ToListAsync();
    }

    public async Task<byte[]> GeneratePdfAsync(PurchaseRegisterFilter filter)
    {
        var rows = await GetDataAsync(filter);
        var totals = CalculateTotals(rows);
        var supplierName = await ResolveSupplierNameAsync(filter);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync();

        var document = new PurchaseRegisterPdfDocument(rows, totals, filter, supplierName, logoImage);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateExcelAsync(PurchaseRegisterFilter filter)
    {
        var rows = await GetDataAsync(filter);
        var totals = CalculateTotals(rows);

        return PurchaseRegisterExcelBuilder.Build(rows, totals);
    }

    private IQueryable<PurchaseRegisterRowDto> BuildQuery(PurchaseRegisterFilter filter)
    {
        var fromDate = filter.FromDate.Date;
        var toDate = filter.ToDate.Date;

        var query = _db.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.InvoiceIssueDate.Date >= fromDate && x.InvoiceIssueDate.Date <= toDate)
            .AsQueryable();

        if (filter.SupplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == filter.SupplierId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.PaymentStatus == filter.Status.Value);
        }

        return query
            .OrderBy(x => x.InvoiceIssueDate)
            .ThenBy(x => x.InvoiceSerie)
            .ThenBy(x => x.InvoiceNumber)
            .Select(x => new PurchaseRegisterRowDto
            {
                InvoiceId = x.Id,
                IssueDate = x.InvoiceIssueDate,
                SupplierName = x.Supplier!.Name,
                SupplierRuc = x.Supplier!.Ruc,
                SerieNumero = x.InvoiceSerie + "-" + x.InvoiceNumber,
                SubTotal = x.Subtotal,
                Igv = x.Igv,
                Total = x.Total,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                AmountPaid = x.AmountPaid,
                Balance = x.Balance,
                Status = x.PaymentStatus.ToString()
            });
    }

    private async Task<string?> ResolveSupplierNameAsync(PurchaseRegisterFilter filter)
    {
        if (!filter.SupplierId.HasValue)
        {
            return null;
        }

        return await _db.Suppliers
            .Where(x => x.Id == filter.SupplierId.Value)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();
    }

    public static PurchaseRegisterTotalsDto CalculateTotals(List<PurchaseRegisterRowDto> rows)
    {
        return new PurchaseRegisterTotalsDto
        {
            CantidadDocumentos = rows.Count,
            TotalCompras = rows.Sum(x => x.Total),
            TotalPagado = rows.Sum(x => x.AmountPaid),
            TotalPendiente = rows.Sum(x => x.Balance)
        };
    }
}

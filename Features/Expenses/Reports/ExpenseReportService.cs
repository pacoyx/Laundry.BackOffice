using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class ExpenseReportService : IExpenseReportService
{
    private readonly RecepcionDbContext _db;
    private readonly ICompanyProfileService _companyProfileService;

    public ExpenseReportService(RecepcionDbContext db, ICompanyProfileService companyProfileService)
    {
        _db = db;
        _companyProfileService = companyProfileService;
    }

    public Task<List<ExpenseDetailRowDto>> GetByPeriodAsync(ExpenseReportFilter filter)
    {
        return BuildDetailQuery(filter).ToListAsync();
    }

    public Task<List<ExpenseDetailRowDto>> GetWithSupportingDocumentsAsync(ExpenseReportFilter filter)
    {
        return BuildDetailQuery(filter, onlyWithSupportingDocument: true).ToListAsync();
    }

    public Task<List<ExpenseAggregateRowDto>> GetByCategoryAsync(ExpenseReportFilter filter)
    {
        return BuildBaseQuery(filter)
            .GroupBy(x => x.ExpenseCategory.Name)
            .Select(g => new ExpenseAggregateRowDto
            {
                Label = g.Key,
                Count = g.Count(),
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();
    }

    public Task<List<ExpenseAggregateRowDto>> GetByBranchAsync(ExpenseReportFilter filter)
    {
        return BuildBaseQuery(filter)
            .GroupBy(x => x.BranchSales.Description)
            .Select(g => new ExpenseAggregateRowDto
            {
                Label = g.Key,
                Count = g.Count(),
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();
    }

    public async Task<byte[]> GenerateDetailPdfAsync(string title, List<ExpenseDetailRowDto> rows, ExpenseReportFilter filter)
    {
        var totals = CalculateTotals(rows);
        var (branchName, categoryName) = await ResolveFilterNamesAsync(filter);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync();

        var document = new ExpenseDetailPdfDocument(title, rows, totals, filter, branchName, categoryName, logoImage);
        return document.GeneratePdf();
    }

    public Task<byte[]> GenerateDetailExcelAsync(List<ExpenseDetailRowDto> rows)
    {
        var totals = CalculateTotals(rows);
        return Task.FromResult(ExpenseDetailExcelBuilder.Build(rows, totals));
    }

    public async Task<byte[]> GenerateAggregatePdfAsync(string title, string labelHeader, List<ExpenseAggregateRowDto> rows, ExpenseReportFilter filter)
    {
        var totals = CalculateAggregateTotals(rows);
        var (branchName, categoryName) = await ResolveFilterNamesAsync(filter);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync();

        var document = new ExpenseAggregatePdfDocument(title, labelHeader, rows, totals, filter, branchName, categoryName, logoImage);
        return document.GeneratePdf();
    }

    public Task<byte[]> GenerateAggregateExcelAsync(string labelHeader, List<ExpenseAggregateRowDto> rows)
    {
        var totals = CalculateAggregateTotals(rows);
        return Task.FromResult(ExpenseAggregateExcelBuilder.Build(labelHeader, rows, totals));
    }

    private IQueryable<Expense> BuildBaseQuery(ExpenseReportFilter filter)
    {
        var fromDate = filter.FromDate.Date;
        var toDate = filter.ToDate.Date;

        var query = _db.Expenses
            .AsNoTracking()
            .Include(x => x.BranchSales)
            .Include(x => x.ExpenseCategory)
            .Where(x => x.IsActive && x.ExpenseDate.Date >= fromDate && x.ExpenseDate.Date <= toDate)
            .AsQueryable();

        if (filter.BranchSalesId.HasValue)
        {
            query = query.Where(x => x.BranchSalesId == filter.BranchSalesId.Value);
        }

        if (filter.ExpenseCategoryId.HasValue)
        {
            query = query.Where(x => x.ExpenseCategoryId == filter.ExpenseCategoryId.Value);
        }

        if (filter.SourceType.HasValue)
        {
            query = query.Where(x => x.SourceType == filter.SourceType.Value);
        }

        return query;
    }

    private IQueryable<ExpenseDetailRowDto> BuildDetailQuery(ExpenseReportFilter filter, bool onlyWithSupportingDocument = false)
    {
        var query = BuildBaseQuery(filter);

        if (onlyWithSupportingDocument)
        {
            query = query.Where(x => x.HasSupportingDocument);
        }

        return query
            .OrderBy(x => x.ExpenseDate)
            .Select(x => new ExpenseDetailRowDto
            {
                Id = x.Id,
                ExpenseDate = x.ExpenseDate,
                BranchName = x.BranchSales.Description,
                CategoryName = x.ExpenseCategory.Name,
                SourceType = x.SourceType.ToString(),
                Amount = x.Amount,
                Description = x.Description,
                HasSupportingDocument = x.HasSupportingDocument,
                DocumentNumber = x.DocumentNumber,
                SupplierName = x.SupplierName
            });
    }

    private async Task<(string? BranchName, string? CategoryName)> ResolveFilterNamesAsync(ExpenseReportFilter filter)
    {
        string? branchName = null;
        string? categoryName = null;

        if (filter.BranchSalesId.HasValue)
        {
            branchName = await _db.BranchSales
                .Where(x => x.Id == filter.BranchSalesId.Value)
                .Select(x => x.Description)
                .FirstOrDefaultAsync();
        }

        if (filter.ExpenseCategoryId.HasValue)
        {
            categoryName = await _db.ExpenseCategories
                .Where(x => x.Id == filter.ExpenseCategoryId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        return (branchName, categoryName);
    }

    public static ExpenseReportTotalsDto CalculateTotals(List<ExpenseDetailRowDto> rows)
    {
        return new ExpenseReportTotalsDto
        {
            Count = rows.Count,
            TotalAmount = rows.Sum(x => x.Amount)
        };
    }

    public static ExpenseReportTotalsDto CalculateAggregateTotals(List<ExpenseAggregateRowDto> rows)
    {
        return new ExpenseReportTotalsDto
        {
            Count = rows.Sum(x => x.Count),
            TotalAmount = rows.Sum(x => x.Amount)
        };
    }
}

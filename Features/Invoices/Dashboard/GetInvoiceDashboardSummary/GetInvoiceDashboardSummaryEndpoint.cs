using Microsoft.EntityFrameworkCore;

public static class GetInvoiceDashboardSummaryEndpoint
{
    public static void MapGetInvoiceDashboardSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var response = await ComputeSummaryAsync(db);

            return Results.Ok(new ApiResponse<InvoiceDashboardResponse>
            {
                Success = true,
                Message = "Resumen del dashboard de facturación.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }

    // Extraido para reutilizarse tambien desde el Management Dashboard sin duplicar la logica.
    internal static async Task<InvoiceDashboardResponse> ComputeSummaryAsync(RecepcionDbContext db)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var monthInvoiceTotals = await db.Invoices
            .AsNoTracking()
            .Where(x => x.IssueDate >= monthStart
                && x.IssueDate < monthEnd
                && x.Status != InvoiceStatus.Cancelled)
            .Select(x => x.Total)
            .ToListAsync();

        var monthCollected = await db.InvoicePayments
            .AsNoTracking()
            .Where(x => x.PaymentDate >= monthStart && x.PaymentDate < monthEnd)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var accountsReceivable = await db.Invoices
            .AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Issued || x.Status == InvoiceStatus.PartiallyPaid)
            .SumAsync(x => (decimal?)x.Balance) ?? 0;

        return new InvoiceDashboardResponse
        {
            MonthInvoiced = monthInvoiceTotals.Sum(),
            MonthCollected = monthCollected,
            AccountsReceivable = accountsReceivable,
            InvoiceCount = monthInvoiceTotals.Count
        };
    }
}

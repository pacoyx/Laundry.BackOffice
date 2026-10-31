using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetInvoicesEndpoint
{
    public static void MapGetInvoices(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            [FromQuery] int? customerId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] InvoiceStatus? status,
            [FromQuery] string? numero,
            RecepcionDbContext db) =>
        {
            var query = db.Invoices
                .AsNoTracking()
                .Include(x => x.Customer)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(numero))
            {
                query = query.Where(x => x.Numero.Contains(numero));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.IssueDate.Date >= dateFrom.Value.Date);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(x => x.IssueDate.Date <= dateTo.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var invoices = await query
                .OrderByDescending(x => x.IssueDate)
                .Select(x => new InvoiceSummaryResponse(
                    x.Id,
                    x.CustomerId,
                    x.Customer.CustomerType == CustomerType.Company && x.Customer.BillingProfile != null
                        ? x.Customer.BillingProfile.BusinessName
                        : (x.Customer.FirtsName + " " + (x.Customer.LastName ?? "")).Trim(),
                    x.Serie,
                    x.Numero,
                    x.CurrencyId,
                    x.ExchangeRate,
                    x.IssueDate,
                    x.DueDate,
                    x.Total,
                    x.AmountPaid,
                    x.Balance,
                    x.PaymentMethod,
                    x.TypePayment,
                    x.DaysCredit,
                    x.Status
                ))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<InvoiceSummaryResponse>>
            {
                Success = true,
                Message = "Listado de facturas.",
                StatusCode = StatusCodes.Status200OK,
                Data = invoices
            });
        });
    }
}

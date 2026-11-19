using Microsoft.EntityFrameworkCore;

public static class GetInvoiceEndpoint
{
    public static void MapGetInvoice(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}", async (long id, RecepcionDbContext db) =>
        {
            var invoice = await db.Invoices
                .AsNoTracking()
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.Details)
                .Include(x => x.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .AsSplitQuery() // Details y Payments son dos colecciones: evita el producto cartesiano (warning EF 20504)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice is null)
            {
                return Results.NotFound();
            }

            var response = new InvoiceResponse(
                invoice.Id,
                invoice.CustomerId,
                invoice.Customer.CustomerType == CustomerType.Company && invoice.Customer.BillingProfile != null
                    ? invoice.Customer.BillingProfile.BusinessName
                    : $"{invoice.Customer.FirtsName} {invoice.Customer.LastName}".Trim(),
                invoice.BranchId,
                invoice.DocumentTypeId,
                invoice.Serie,
                invoice.Numero,
                invoice.CurrencyId,
                invoice.ExchangeRate,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.SubTotal,
                invoice.Igv,
                invoice.Total,
                invoice.AmountPaid,
                invoice.Balance,
                invoice.PaymentMethod,
                invoice.TypePayment,
                invoice.DaysCredit,
                invoice.Status,
                invoice.Notes,
                invoice.Details.Select(d => new InvoiceDetailItemResponse(
                    d.Id, d.ProductId, d.Description, d.Quantity, d.UnitPrice, d.Discount, d.Total
                )).ToList(),
                invoice.Payments.Select(p => new InvoicePaymentResponse(
                    p.Id, p.InvoiceId, p.PaymentMethodId, p.PaymentMethod.Name, p.PaymentDate,
                    p.Amount, p.OperationNumber, p.Observation, p.CollectionSheetId
                )).ToList()
            );

            return Results.Ok(new ApiResponse<InvoiceResponse>
            {
                Success = true,
                Message = "Factura encontrada.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }
}

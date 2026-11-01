using FluentValidation;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public static class CreateInvoiceEndpoint
{
    public static void MapCreateInvoice(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateInvoiceRequest request,
            IValidator<CreateInvoiceRequest> validator,
            RecepcionDbContext db,
            IOptions<InvoiceSettings> invoiceSettings,
            IOptions<ElectronicInvoicingSettings> electronicInvoicingSettings,
            IBackgroundJobClient backgroundJobClient) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var customerExists = await db.Customers.AnyAsync(x => x.Id == request.CustomerId);
            if (!customerExists)
            {
                return Results.NotFound("Cliente no encontrado.");
            }

            var serieNumeroEnUso = await db.Invoices.AnyAsync(x =>
                x.Serie == request.Serie && x.Numero == request.Numero);
            if (serieNumeroEnUso)
            {
                return Results.BadRequest("Ya existe una factura registrada con esa serie y número.");
            }

            var details = request.Details.Select(d => new InvoiceDetail
            {
                ProductId = d.ProductId,
                Description = d.Description,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Discount = d.Discount,
                Total = (d.Quantity * d.UnitPrice) - d.Discount
            }).ToList();

            var subTotal = details.Sum(d => d.Total);
            var igv = Math.Round(subTotal * invoiceSettings.Value.IgvRate, 2);
            var total = subTotal + igv;

            var invoice = new Invoice
            {
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                DocumentTypeId = request.DocumentTypeId,
                Serie = request.Serie,
                Numero = request.Numero,
                CurrencyId = request.CurrencyId,
                ExchangeRate = request.ExchangeRate,
                IssueDate = request.IssueDate,
                DueDate = request.DueDate,
                PaymentMethod = request.PaymentMethod,
                TypePayment = request.TypePayment,
                DaysCredit = request.DaysCredit,
                Notes = request.Notes,
                SubTotal = subTotal,
                Igv = igv,
                Total = total,
                AmountPaid = 0,
                Balance = total,
                Status = InvoiceStatus.Issued,
                Details = details
            };

            await db.Invoices.AddAsync(invoice);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // El check de arriba (AnyAsync) no evita la carrera entre dos requests casi
                // simultaneos con la misma Serie+Numero; el indice unico la ataja aca.
                return Results.BadRequest(new[] { "Ya existe una factura registrada con esa serie y número. Verifica e intenta de nuevo." });
            }

            if (string.Equals(electronicInvoicingSettings.Value.Provider, "NubeFact", StringComparison.OrdinalIgnoreCase))
            {
                backgroundJobClient.Enqueue<IElectronicInvoicingService>(x => x.SubmitInvoiceAsync(invoice.Id, CancellationToken.None));
            }

            var response = new ApiResponse<CreateInvoiceResponse>
            {
                Success = true,
                Message = "Factura registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = new CreateInvoiceResponse(
                    invoice.Id,
                    invoice.Serie,
                    invoice.Numero,
                    invoice.CurrencyId,
                    invoice.ExchangeRate,
                    invoice.SubTotal,
                    invoice.Igv,
                    invoice.Total,
                    invoice.PaymentMethod,
                    invoice.TypePayment,
                    invoice.DaysCredit,
                    invoice.Status)
            };

            return Results.Created($"/api/invoices/{invoice.Id}", response);
        })
        .RequireAuthorization();
    }
}

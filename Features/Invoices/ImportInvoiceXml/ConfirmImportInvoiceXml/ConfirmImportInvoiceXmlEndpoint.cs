using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class ConfirmImportInvoiceXmlEndpoint
{
    public static void MapConfirmImportInvoiceXml(this IEndpointRouteBuilder app)
    {
        app.MapPost("/import-xml", async (
            ConfirmImportInvoiceXmlRequest request,
            IValidator<ConfirmImportInvoiceXmlRequest> validator,
            RecepcionDbContext db) =>
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
                return Results.BadRequest(new[] { "Ya existe una factura registrada con esa serie y número." });
            }

            // A diferencia de CreateInvoiceEndpoint, NO se recalculan los montos: el XML ya es
            // el documento legal aceptado por SUNAT, sus totales son la fuente de verdad.
            var details = request.Details.Select(d => new InvoiceDetail
            {
                ProductId = d.ProductId,
                Description = d.Description,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Discount = 0,
                Total = d.Total
            }).ToList();

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
                SubTotal = request.SubTotal,
                Igv = request.Igv,
                Total = request.Total,
                AmountPaid = 0,
                Balance = request.Total,
                Status = InvoiceStatus.Issued,
                HasDetraction = request.HasDetraction,
                DetractionPercent = request.HasDetraction ? request.DetractionPercent : null,
                DetractionAmount = request.HasDetraction ? request.DetractionAmount : null,
                Details = details
            };

            await db.Invoices.AddAsync(invoice);
            await db.SaveChangesAsync();

            var response = new ApiResponse<CreateInvoiceResponse>
            {
                Success = true,
                Message = "Factura importada correctamente desde el XML.",
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
        .WithName("ConfirmImportInvoiceXml")
        .Produces<ApiResponse<CreateInvoiceResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

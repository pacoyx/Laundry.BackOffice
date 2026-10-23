using Microsoft.EntityFrameworkCore;

public static class PreviewImportInvoiceXmlEndpoint
{
    public static void MapPreviewImportInvoiceXml(this IEndpointRouteBuilder app)
    {
        app.MapPost("/import-xml/preview", async (IFormFile file, RecepcionDbContext db, CancellationToken cancellationToken) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            ParsedSunatInvoice parsed;
            try
            {
                await using var stream = file.OpenReadStream();
                parsed = SunatInvoiceXmlParser.Parse(stream);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new[] { $"No se pudo leer el XML: {ex.Message}" });
            }

            var customer = await db.Customers
                .AsNoTracking()
                .Include(x => x.BillingProfile)
                .FirstOrDefaultAsync(x =>
                    x.DocPersonal == parsed.CustomerDocId ||
                    (x.BillingProfile != null && x.BillingProfile.TaxId == parsed.CustomerDocId),
                    cancellationToken);

            if (customer is null)
            {
                return Results.BadRequest(new[]
                {
                    $"No se encontró un cliente registrado con RUC/DNI '{parsed.CustomerDocId}' ({parsed.CustomerName}). Registra el cliente antes de importar."
                });
            }

            var documentType = await db.TypeDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DocId == parsed.InvoiceTypeCode, cancellationToken);

            if (documentType is null)
            {
                return Results.BadRequest(new[] { $"No se encontró un tipo de documento configurado para el código SUNAT '{parsed.InvoiceTypeCode}'." });
            }

            var duplicate = await db.Invoices.AnyAsync(x => x.Serie == parsed.Serie && x.Numero == parsed.Numero, cancellationToken);
            if (duplicate)
            {
                return Results.BadRequest(new[] { $"Ya existe una factura registrada con serie '{parsed.Serie}' y número '{parsed.Numero}'." });
            }

            var detractionAccountMatches = true;
            string? detractionWarning = null;

            if (parsed.HasDetraction)
            {
                var detractionAccount = await db.CompanyBankAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IsDetractionAccount && x.IsEnabled, cancellationToken);

                if (detractionAccount is null)
                {
                    detractionAccountMatches = false;
                    detractionWarning = "El XML trae detracción, pero no hay ninguna cuenta bancaria marcada como cuenta de detracción en el sistema.";
                }
                else if (!string.IsNullOrWhiteSpace(parsed.DetractionAccountNumber) &&
                         !string.Equals(detractionAccount.AccountNumber, parsed.DetractionAccountNumber, StringComparison.Ordinal))
                {
                    detractionAccountMatches = false;
                    detractionWarning = $"La cuenta de detracción del XML ('{parsed.DetractionAccountNumber}') no coincide con la cuenta configurada en el sistema ('{detractionAccount.AccountNumber}').";
                }
            }

            var daysCredit = parsed.TypePayment == "credito" ? Math.Max(0, (parsed.DueDate - parsed.IssueDate).Days) : 0;

            var response = new ImportInvoiceXmlPreviewResponse(
                customer.Id,
                PickupGuidesEndpoints.GetCustomerDisplayName(customer),
                documentType.Id,
                parsed.Serie,
                parsed.Numero,
                parsed.IssueDate,
                parsed.DueDate,
                parsed.CurrencyId,
                parsed.TypePayment,
                daysCredit,
                parsed.SubTotal,
                parsed.Igv,
                parsed.Total,
                parsed.HasDetraction,
                parsed.DetractionPercent,
                parsed.DetractionAmount,
                detractionAccountMatches,
                detractionWarning,
                parsed.Lines
                    .Select((l, i) => new ImportInvoiceXmlLineResponse(i + 1, l.Description, l.Quantity, l.UnitPrice, l.Total, l.SellerItemCode))
                    .ToList()
            );

            return Results.Ok(new ApiResponse<ImportInvoiceXmlPreviewResponse>
            {
                Success = true,
                Message = "Vista previa generada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .DisableAntiforgery()
        .WithName("PreviewImportInvoiceXml")
        .Produces<ApiResponse<ImportInvoiceXmlPreviewResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

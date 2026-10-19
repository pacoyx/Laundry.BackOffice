using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class InvoicePdfDocument : IDocument
{
    private readonly Invoice _invoice;
    private readonly Company? _company;
    private readonly InvoicePrintSettings _invoicePrint;
    private readonly string? _branchAddress;
    private readonly byte[]? _logoImage;

    public InvoicePdfDocument(Invoice invoice, Company? company, InvoicePrintSettings invoicePrint, string? branchAddress, byte[]? logoImage = null)
    {
        _invoice = invoice;
        _company = company;
        _invoicePrint = invoicePrint;
        _branchAddress = branchAddress;
        _logoImage = logoImage;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(8.5f));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (_logoImage is not null)
                {
                    row.ConstantItem(55).Height(55).Image(_logoImage).FitArea();
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(CompanyTradeName).Bold().FontSize(12);
                    if (!string.IsNullOrWhiteSpace(CompanyLegalName))
                    {
                        col.Item().Text(CompanyLegalName).Bold().FontSize(9);
                    }
                    col.Item().Text(CompanyAddress).FontSize(8);
                });

                row.ConstantItem(170).Border(1).BorderColor(Colors.Grey.Darken1).Padding(6).Column(col =>
                {
                    col.Item().AlignCenter().Text("FACTURA ELECTRÓNICA").Bold().FontSize(10);
                    col.Item().AlignCenter().PaddingTop(2).Text($"{CompanyTaxIdLabel}: {CompanyTaxId}").FontSize(9);
                    col.Item().AlignCenter().PaddingTop(2).Text($"{_invoice.Serie}-{_invoice.Numero}").Bold().FontSize(13);
                });
            });

            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            ComposeInvoiceInfo(column);
            ComposeItemsTable(column);
            ComposeTotals(column);

            if (_invoice.Total > _invoicePrint.Detraction.MinimumAmount)
            {
                column.Item().PaddingTop(10).Element(c => ComposeDetraction(c));
            }

            if (string.Equals(_invoice.TypePayment, "credito", StringComparison.OrdinalIgnoreCase))
            {
                column.Item().PaddingTop(10).Element(c => ComposeCreditInfo(c));
            }
        });
    }

    private void ComposeInvoiceInfo(ColumnDescriptor column)
    {
        var customer = _invoice.Customer;

        column.Item().PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Fecha de Emisión: ").Bold();
                text.Span(_invoice.IssueDate.ToString("dd/MM/yyyy"));
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Forma de pago: ").Bold();
                text.Span(GetPaymentTypeDisplay(_invoice.TypePayment));
            });
        });

        column.Item().PaddingTop(4).Text(text =>
        {
            text.Span("Señor(es): ").Bold();
            text.Span(GetCustomerName(customer));
        });
        column.Item().Text(text =>
        {
            text.Span("RUC / DNI: ").Bold();
            text.Span(GetCustomerTaxId(customer));
        });
        column.Item().Text(text =>
        {
            text.Span("Dirección del receptor: ").Bold();
            text.Span(GetCustomerAddress(customer));
        });
        column.Item().Text(text =>
        {
            text.Span("Establecimiento del emisor: ").Bold();
            text.Span(string.IsNullOrWhiteSpace(_branchAddress) ? CompanyAddress : _branchAddress);
        });
        column.Item().Text(text =>
        {
            text.Span("Tipo de moneda: ").Bold();
            text.Span(GetCurrencyName(_invoice.CurrencyId));
        });

        if (!string.IsNullOrWhiteSpace(_invoicePrint.PaymentAccountNote))
        {
            column.Item().Text(text =>
            {
                text.Span("Observación: ").Bold();
                text.Span(_invoicePrint.PaymentAccountNote);
            });
        }

        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
    }

    private void ComposeItemsTable(ColumnDescriptor column)
    {
        column.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(4f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Cantidad");
                header.Cell().Element(HeaderCellStyle).Text("Unidad Medida");
                header.Cell().Element(HeaderCellStyle).Text("Código");
                header.Cell().Element(HeaderCellStyle).Text("Descripción");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Valor Unitario");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("ICBPER");
            });

            foreach (var detail in _invoice.Details)
            {
                table.Cell().Element(CellStyle).Text(detail.Quantity.ToString("N2"));
                table.Cell().Element(CellStyle).Text("UNIDAD");
                table.Cell().Element(CellStyle).Text(detail.ProductId.ToString());
                table.Cell().Element(CellStyle).Text(detail.Description);
                table.Cell().Element(CellStyle).AlignRight().Text(detail.UnitPrice.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text("0.00");
            }
        });
    }

    private void ComposeTotals(ColumnDescriptor column)
    {
        var currencySymbol = GetCurrencySymbol(_invoice.CurrencyId);
        var amountInWords = SpanishAmountInWordsConverter.Convert(_invoice.Total, _invoice.CurrencyId);

        column.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem(1.2f).Column(col =>
            {
                col.Item().Text(text =>
                {
                    text.Span("Valor de Venta de Operaciones Gratuitas: ").Bold();
                    text.Span($"{currencySymbol} 0.00");
                });
                col.Item().PaddingTop(12).Text($"SON: {amountInWords}").Bold();
            });

            row.RelativeItem(1f).Border(1).BorderColor(Colors.Grey.Darken1).Padding(6).Column(col =>
            {
                AddTotalLine(col, "Sub Total Ventas", _invoice.SubTotal, currencySymbol);
                AddTotalLine(col, "Anticipos", 0, currencySymbol);
                AddTotalLine(col, "Descuentos", 0, currencySymbol);
                AddTotalLine(col, "Valor Venta", _invoice.SubTotal, currencySymbol);
                AddTotalLine(col, "ISC", 0, currencySymbol);
                AddTotalLine(col, "IGV", _invoice.Igv, currencySymbol);
                AddTotalLine(col, "ICBPER", 0, currencySymbol);
                AddTotalLine(col, "Otros Cargos", 0, currencySymbol);
                AddTotalLine(col, "Otros Tributos", 0, currencySymbol);
                AddTotalLine(col, "Monto de redondeo", 0, currencySymbol);
                col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                AddTotalLine(col, "Importe Total", _invoice.Total, currencySymbol, bold: true);
            });
        });
    }

    private void ComposeDetraction(IContainer container)
    {
        var detraction = _invoicePrint.Detraction;
        var detractionAmount = Math.Round(_invoice.Total * detraction.Percentage / 100m, 2);

        container.Border(1).BorderColor(Colors.Grey.Darken1).Padding(6).Column(col =>
        {
            col.Item().Text("Información de la detracción").Bold().FontSize(9);
            col.Item().PaddingTop(3).Text(text =>
            {
                text.Span("Leyenda: ").Bold();
                text.Span(detraction.Legend);
            });
            col.Item().Text(text =>
            {
                text.Span("Bien o Servicio: ").Bold();
                text.Span($"{detraction.BienServicioCode} {detraction.BienServicioDescription}");
            });
            col.Item().Text(text =>
            {
                text.Span("Medio de pago: ").Bold();
                text.Span($"{detraction.MedioPagoCode} {detraction.MedioPagoDescription}");
            });
            col.Item().PaddingTop(2).Row(row =>
            {
                row.RelativeItem(1.5f).Text(text =>
                {
                    text.Span("Nro. Cta. Banco de la Nación: ").Bold();
                    text.Span(detraction.BankAccountNumber);
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Porcentaje de detracción: ").Bold();
                    text.Span($"{detraction.Percentage:N2}%");
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Monto detracción: ").Bold();
                    text.Span($"{GetCurrencySymbol(_invoice.CurrencyId)} {detractionAmount:N2}");
                });
            });
        });
    }

    private void ComposeCreditInfo(IContainer container)
    {
        var currencySymbol = GetCurrencySymbol(_invoice.CurrencyId);

        container.Border(1).BorderColor(Colors.Grey.Darken1).Padding(6).Column(col =>
        {
            col.Item().Text("Información del crédito").Bold().FontSize(9);
            col.Item().PaddingTop(3).Text(text =>
            {
                text.Span("Monto neto pendiente de pago: ").Bold();
                text.Span($"{currencySymbol} {_invoice.Balance:N2}");
            });
            col.Item().Text(text =>
            {
                text.Span("Total de Cuotas: ").Bold();
                text.Span("1");
            });

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Nº Cuota");
                    header.Cell().Element(HeaderCellStyle).Text("Fec. Venc.");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Monto");
                });

                table.Cell().Element(CellStyle).Text("1");
                table.Cell().Element(CellStyle).Text(_invoice.DueDate.ToString("dd/MM/yyyy"));
                table.Cell().Element(CellStyle).AlignRight().Text(_invoice.Balance.ToString("N2"));
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(5).AlignCenter().Text(
                "Documento referencial generado por el sistema interno de facturación.").FontSize(7).Italic();
            column.Item().PaddingTop(3).AlignRight().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    private static void AddTotalLine(ColumnDescriptor column, string label, decimal value, string currencySymbol, bool bold = false)
    {
        column.Item().Row(row =>
        {
            if (bold)
            {
                row.RelativeItem().Text(label).FontSize(9).Bold();
                row.RelativeItem().AlignRight().Text($"{currencySymbol} {value:N2}").FontSize(9).Bold();
            }
            else
            {
                row.RelativeItem().Text(label).FontSize(8);
                row.RelativeItem().AlignRight().Text($"{currencySymbol} {value:N2}").FontSize(8);
            }
        });
    }

    private static IContainer HeaderCellStyle(IContainer c) =>
        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
         .Background(Colors.Blue.Darken2)
         .Padding(4);

    private static IContainer CellStyle(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);

    private static string GetPaymentTypeDisplay(string typePayment) => typePayment.ToLowerInvariant() switch
    {
        "credito" => "Crédito",
        "contado" => "Contado",
        _ => typePayment
    };

    private static string GetCurrencyName(string currencyId) => currencyId switch
    {
        "PEN" => "SOLES",
        "USD" => "DÓLARES AMERICANOS",
        _ => currencyId
    };

    private static string GetCurrencySymbol(string currencyId) => currencyId switch
    {
        "PEN" => "S/",
        "USD" => "US$",
        _ => currencyId
    };

    private static string GetCustomerName(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.BusinessName
            : $"{customer.FirtsName} {customer.LastName}".Trim();

    private static string GetCustomerTaxId(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.TaxId
            : customer.DocPersonal ?? "-";

    private static string GetCustomerAddress(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.FiscalAddress
            : customer.Address ?? "-";

    private string CompanyTradeName => _company?.NameComercial ?? string.Empty;

    private string CompanyLegalName => _company?.NameCompany ?? string.Empty;

    private string CompanyTaxIdLabel => string.IsNullOrWhiteSpace(_company?.DocumentType) ? "RUC" : _company.DocumentType;

    private string CompanyTaxId => _company?.NumberType ?? string.Empty;

    private string CompanyAddress => _company?.Address ?? string.Empty;
}

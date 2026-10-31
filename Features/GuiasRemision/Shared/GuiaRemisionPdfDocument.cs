using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class GuiaRemisionPdfDocument : IDocument
{
    private readonly GuiaRemision _guia;
    private readonly Company? _company;
    private readonly byte[]? _logoImage;

    public GuiaRemisionPdfDocument(GuiaRemision guia, Company? company, byte[]? logoImage = null)
    {
        _guia = guia;
        _company = company;
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
                    col.Item().AlignCenter().Text("GUÍA DE REMISIÓN").Bold().FontSize(10);
                    col.Item().AlignCenter().PaddingTop(2).Text($"{CompanyTaxIdLabel}: {CompanyTaxId}").FontSize(9);
                    col.Item().AlignCenter().PaddingTop(2).Text($"{_guia.Serie}-{_guia.Numero}").Bold().FontSize(13);
                });
            });

            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            ComposeGuiaInfo(column);
            ComposeItemsTable(column);
        });
    }

    private void ComposeGuiaInfo(ColumnDescriptor column)
    {
        var customer = _guia.Invoice.Customer;

        column.Item().PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Fecha de Emisión: ").Bold();
                text.Span(_guia.FechaEmision.ToString("dd/MM/yyyy"));
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Fecha de Traslado: ").Bold();
                text.Span(_guia.FechaTraslado.ToString("dd/MM/yyyy"));
            });
        });

        column.Item().PaddingTop(4).Text(text =>
        {
            text.Span("Destinatario: ").Bold();
            text.Span(GetCustomerName(customer));
        });
        column.Item().Text(text =>
        {
            text.Span("RUC / DNI: ").Bold();
            text.Span(GetCustomerTaxId(customer));
        });
        column.Item().Text(text =>
        {
            text.Span("Comprobante de referencia: ").Bold();
            text.Span($"{_guia.Invoice.Serie}-{_guia.Invoice.Numero}");
        });
        column.Item().Text(text =>
        {
            text.Span("Motivo de traslado: ").Bold();
            text.Span(_guia.MotivoTraslado);
        });

        column.Item().PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Punto de partida: ").Bold();
                text.Span(_guia.OriginAddress);
            });
        });
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Punto de llegada: ").Bold();
                text.Span(_guia.DeliveryAddress);
            });
        });

        column.Item().PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Conductor: ").Bold();
                text.Span(_guia.DriverName);
            });
            row.RelativeItem().Text(text =>
            {
                text.Span("DNI: ").Bold();
                text.Span(_guia.DriverDni);
            });
            row.RelativeItem().Text(text =>
            {
                text.Span("Placa: ").Bold();
                text.Span(_guia.Vehicle?.Plate ?? "-");
            });
        });

        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
    }

    private void ComposeItemsTable(ColumnDescriptor column)
    {
        column.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1f);
                columns.RelativeColumn(6f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Cantidad");
                header.Cell().Element(HeaderCellStyle).Text("Descripción");
            });

            foreach (var detail in _guia.Invoice.Details)
            {
                table.Cell().Element(CellStyle).Text(detail.Quantity.ToString("N2"));
                table.Cell().Element(CellStyle).Text(detail.Description);
            }
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

    private static IContainer HeaderCellStyle(IContainer c) =>
        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
         .Background(Colors.Blue.Darken2)
         .Padding(4);

    private static IContainer CellStyle(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);

    private static string GetCustomerName(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.BusinessName
            : $"{customer.FirtsName} {customer.LastName}".Trim();

    private static string GetCustomerTaxId(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.TaxId
            : customer.DocPersonal ?? "-";

    private string CompanyTradeName => _company?.NameComercial ?? string.Empty;

    private string CompanyLegalName => _company?.NameCompany ?? string.Empty;

    private string CompanyTaxIdLabel => string.IsNullOrWhiteSpace(_company?.DocumentType) ? "RUC" : _company.DocumentType;

    private string CompanyTaxId => _company?.NumberType ?? string.Empty;

    private string CompanyAddress => _company?.Address ?? string.Empty;
}

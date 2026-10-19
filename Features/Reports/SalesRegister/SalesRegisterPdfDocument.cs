using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class SalesRegisterPdfDocument : IDocument
{
    private readonly List<SalesRegisterRowDto> _rows;
    private readonly SalesRegisterTotalsDto _totals;
    private readonly SalesRegisterFilter _filter;
    private readonly string? _branchName;
    private readonly string? _customerName;
    private readonly byte[]? _logoImage;
    private readonly DateTime _generatedAt;

    public SalesRegisterPdfDocument(
        List<SalesRegisterRowDto> rows,
        SalesRegisterTotalsDto totals,
        SalesRegisterFilter filter,
        string? branchName,
        string? customerName,
        byte[]? logoImage = null)
    {
        _rows = rows;
        _totals = totals;
        _filter = filter;
        _branchName = branchName;
        _customerName = customerName;
        _logoImage = logoImage;
        _generatedAt = DateTime.Now;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(9));

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
                    row.ConstantItem(60).Height(60).Image(_logoImage).FitArea();
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("REGISTRO DE VENTAS").FontSize(16).Bold();
                    col.Item().AlignCenter().Text($"Generado el: {_generatedAt:dd/MM/yyyy HH:mm}").FontSize(8);
                });
            });

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Desde: ").Bold();
                    text.Span(_filter.FromDate.ToString("dd/MM/yyyy"));
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Hasta: ").Bold();
                    text.Span(_filter.ToDate.ToString("dd/MM/yyyy"));
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Sucursal: ").Bold();
                    text.Span(_branchName ?? "Todas");
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Cliente: ").Bold();
                    text.Span(_customerName ?? "Todos");
                });
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1.2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Fecha");
                header.Cell().Element(HeaderCellStyle).Text("Cliente");
                header.Cell().Element(HeaderCellStyle).Text("Documento");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("SubTotal");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("IGV");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Pagado");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Saldo");
                header.Cell().Element(HeaderCellStyle).Text("Moneda");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("T. Cambio");
                header.Cell().Element(HeaderCellStyle).Text("Forma Pago");
                header.Cell().Element(HeaderCellStyle).Text("Tipo Pago");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Días Créd.");
                header.Cell().Element(HeaderCellStyle).Text("Estado");

                static IContainer HeaderCellStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                     .Background(Colors.Blue.Darken2)
                     .Padding(4);
            });

            foreach (var row in _rows)
            {
                table.Cell().Element(CellStyle).Text(row.IssueDate.ToString("dd/MM/yyyy"));
                table.Cell().Element(CellStyle).Text(row.CustomerName);
                table.Cell().Element(CellStyle).Text(row.SerieNumero);
                table.Cell().Element(CellStyle).AlignRight().Text(row.SubTotal.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(row.Igv.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(row.Total.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(row.AmountPaid.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(row.Balance.ToString("N2"));
                table.Cell().Element(CellStyle).Text(row.CurrencyId);
                table.Cell().Element(CellStyle).AlignRight().Text(row.ExchangeRate.ToString("N4"));
                table.Cell().Element(CellStyle).Text(row.PaymentMethod);
                table.Cell().Element(CellStyle).Text(row.TypePayment);
                table.Cell().Element(CellStyle).AlignRight().Text(row.DaysCredit.ToString());
                table.Cell().Element(CellStyle).Text(row.Status);
            }

            static IContainer CellStyle(IContainer c) =>
                c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Cantidad documentos: ").Bold();
                    text.Span(_totals.CantidadDocumentos.ToString());
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total vendido: ").Bold();
                    text.Span(_totals.TotalVentas.ToString("N2"));
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total cobrado: ").Bold();
                    text.Span(_totals.TotalCobrado.ToString("N2"));
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Saldo pendiente: ").Bold();
                    text.Span(_totals.TotalPendiente.ToString("N2"));
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });
    }
}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ValuationReportSummaryPdfDocument : IDocument
{
    private const string LogoPath = "wwwroot/images/logo.png";

    private readonly ValuationReportPivotDto _pivot;
    private readonly string _customerName;
    private readonly ValuationReportFilter _filter;
    private readonly DateTime _generatedAt;

    public ValuationReportSummaryPdfDocument(ValuationReportPivotDto pivot, string customerName, ValuationReportFilter filter)
    {
        _pivot = pivot;
        _customerName = customerName;
        _filter = filter;
        _generatedAt = DateTime.Now;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(8));

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
                if (File.Exists(LogoPath))
                {
                    row.ConstantItem(60).Height(60).Image(LogoPath).FitArea();
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("VALORIZACIÓN DEL MES - RESUMEN DE SERVICIOS").FontSize(14).Bold();
                    col.Item().AlignCenter().Text($"Generado el: {_generatedAt:dd/MM/yyyy HH:mm}").FontSize(8);
                });
            });

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Cliente: ").Bold();
                    text.Span(_customerName);
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Periodo: ").Bold();
                    text.Span($"{_filter.FromDate:dd/MM/yyyy} - {_filter.ToDate:dd/MM/yyyy}");
                });
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_pivot.Services.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontraron servicios registrados en el periodo seleccionado.").Italic();
            return;
        }

        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.3f);
                foreach (var _ in _pivot.Services)
                {
                    columns.RelativeColumn(1f);
                }
                columns.RelativeColumn(1.1f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Fecha");
                foreach (var service in _pivot.Services)
                {
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text(service.ServiceName);
                }
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total");

                static IContainer HeaderCellStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                     .Background(Colors.Blue.Darken2)
                     .Padding(4);
            });

            foreach (var row in _pivot.Rows)
            {
                table.Cell().Element(CellStyle).Text(row.Date.ToString("dd/MM/yyyy"));
                foreach (var service in _pivot.Services)
                {
                    var value = row.QuantitiesByServiceItemId.GetValueOrDefault(service.ServiceItemId);
                    table.Cell().Element(CellStyle).AlignRight().Text(value == 0 ? "-" : value.ToString("N2"));
                }
                table.Cell().Element(CellStyle).AlignRight().Text(row.RowTotal.ToString("N2"));
            }

            table.Cell().Element(TotalCellStyle).Text("TOTAL");
            foreach (var service in _pivot.Services)
            {
                var columnTotal = _pivot.ColumnTotals.GetValueOrDefault(service.ServiceItemId);
                table.Cell().Element(TotalCellStyle).AlignRight().Text(columnTotal.ToString("N2"));
            }
            table.Cell().Element(TotalCellStyle).AlignRight().Text(_pivot.GrandTotal.ToString("N2"));

            static IContainer CellStyle(IContainer c) =>
                c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);

            static IContainer TotalCellStyle(IContainer c) =>
                c.DefaultTextStyle(x => x.Bold())
                 .Background(Colors.Grey.Lighten3)
                 .BorderTop(1).BorderColor(Colors.Grey.Darken1)
                 .PaddingVertical(4).PaddingHorizontal(4);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(5).AlignRight().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }
}

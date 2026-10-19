using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ExpenseAggregatePdfDocument : IDocument
{
    private readonly string _title;
    private readonly string _labelHeader;
    private readonly List<ExpenseAggregateRowDto> _rows;
    private readonly ExpenseReportTotalsDto _totals;
    private readonly ExpenseReportFilter _filter;
    private readonly string? _branchName;
    private readonly string? _categoryName;
    private readonly byte[]? _logoImage;
    private readonly DateTime _generatedAt;

    public ExpenseAggregatePdfDocument(
        string title,
        string labelHeader,
        List<ExpenseAggregateRowDto> rows,
        ExpenseReportTotalsDto totals,
        ExpenseReportFilter filter,
        string? branchName,
        string? categoryName,
        byte[]? logoImage = null)
    {
        _title = title;
        _labelHeader = labelHeader;
        _rows = rows;
        _totals = totals;
        _filter = filter;
        _branchName = branchName;
        _categoryName = categoryName;
        _logoImage = logoImage;
        _generatedAt = DateTime.Now;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10));

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
                    col.Item().AlignCenter().Text(_title).FontSize(16).Bold();
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
                    text.Span("Categoría: ").Bold();
                    text.Span(_categoryName ?? "Todas");
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
                columns.RelativeColumn(3f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text(_labelHeader);
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cantidad");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Monto");

                static IContainer HeaderCellStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                     .Background(Colors.Blue.Darken2)
                     .Padding(4);
            });

            foreach (var row in _rows)
            {
                table.Cell().Element(CellStyle).Text(row.Label);
                table.Cell().Element(CellStyle).AlignRight().Text(row.Count.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text(row.Amount.ToString("N2"));
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
                    text.Span("Cantidad de gastos: ").Bold();
                    text.Span(_totals.Count.ToString());
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Total: ").Bold();
                    text.Span(_totals.TotalAmount.ToString("N2"));
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

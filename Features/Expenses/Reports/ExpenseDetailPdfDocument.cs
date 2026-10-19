using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ExpenseDetailPdfDocument : IDocument
{
    private readonly string _title;
    private readonly List<ExpenseDetailRowDto> _rows;
    private readonly ExpenseReportTotalsDto _totals;
    private readonly ExpenseReportFilter _filter;
    private readonly string? _branchName;
    private readonly string? _categoryName;
    private readonly byte[]? _logoImage;
    private readonly DateTime _generatedAt;

    public ExpenseDetailPdfDocument(
        string title,
        List<ExpenseDetailRowDto> rows,
        ExpenseReportTotalsDto totals,
        ExpenseReportFilter filter,
        string? branchName,
        string? categoryName,
        byte[]? logoImage = null)
    {
        _title = title;
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
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(1.6f);
                columns.RelativeColumn(1.4f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(2.8f);
                columns.RelativeColumn(1.4f);
                columns.RelativeColumn(1.8f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Fecha");
                header.Cell().Element(HeaderCellStyle).Text("Sucursal");
                header.Cell().Element(HeaderCellStyle).Text("Categoría");
                header.Cell().Element(HeaderCellStyle).Text("Origen");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Monto");
                header.Cell().Element(HeaderCellStyle).Text("Descripción");
                header.Cell().Element(HeaderCellStyle).Text("Documento");
                header.Cell().Element(HeaderCellStyle).Text("Proveedor");

                static IContainer HeaderCellStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                     .Background(Colors.Blue.Darken2)
                     .Padding(4);
            });

            foreach (var row in _rows)
            {
                table.Cell().Element(CellStyle).Text(row.ExpenseDate.ToString("dd/MM/yyyy"));
                table.Cell().Element(CellStyle).Text(row.BranchName);
                table.Cell().Element(CellStyle).Text(row.CategoryName);
                table.Cell().Element(CellStyle).Text(row.SourceType);
                table.Cell().Element(CellStyle).AlignRight().Text(row.Amount.ToString("N2"));
                table.Cell().Element(CellStyle).Text(row.Description);
                table.Cell().Element(CellStyle).Text(row.DocumentNumber ?? "-");
                table.Cell().Element(CellStyle).Text(row.SupplierName ?? "-");
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

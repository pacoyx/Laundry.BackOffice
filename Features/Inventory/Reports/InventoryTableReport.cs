using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public enum ReportColumnKind { Text, Quantity, Money, Date }

public record ReportColumn(string Header, ReportColumnKind Kind = ReportColumnKind.Text, float Width = 1f);

/// <summary>
/// Reporte tabular genérico de inventario (título, filtros, columnas, filas y totales) que se exporta
/// tanto a Excel como a PDF, para no repetir un generador por cada reporte.
/// </summary>
public class InventoryTableReport
{
    public string Title { get; init; } = string.Empty;
    public List<string> Subtitles { get; init; } = new();
    public List<ReportColumn> Columns { get; init; } = new();
    public List<object?[]> Rows { get; init; } = new();
    public object?[]? Totals { get; init; }
    public bool Landscape { get; init; } = true;

    private const string HeaderBg = "#1F4E78";

    // ---------------- Excel ----------------

    public byte[] ToExcel(string sheetName)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName.Length > 31 ? sheetName[..31] : sheetName);

        sheet.Cell(1, 1).Value = Title;
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(XLColor.FromHtml(HeaderBg));
        var row = 2;
        foreach (var subtitle in Subtitles)
        {
            sheet.Cell(row++, 1).Value = subtitle;
        }
        sheet.Cell(row++, 1).Value = $"Generado el {InventoryPostingService.NowLima():dd/MM/yyyy HH:mm}";
        sheet.Cell(row - 1, 1).Style.Font.SetItalic().Font.SetFontSize(9);
        row++;

        var headerRow = row;
        for (var c = 0; c < Columns.Count; c++)
        {
            var cell = sheet.Cell(headerRow, c + 1);
            cell.Value = Columns[c].Header;
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(HeaderBg));
            cell.Style.Alignment.SetHorizontal(Columns[c].Kind == ReportColumnKind.Text ? XLAlignmentHorizontalValues.Left : XLAlignmentHorizontalValues.Right);
        }

        row = headerRow + 1;
        foreach (var values in Rows)
        {
            WriteExcelRow(sheet, row++, values);
        }
        var lastDataRow = row - 1;

        if (Totals is not null)
        {
            WriteExcelRow(sheet, row, Totals);
            var totals = sheet.Range(row, 1, row, Columns.Count);
            totals.Style.Font.SetBold();
            totals.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9D9D9"));
            totals.Style.Border.SetTopBorder(XLBorderStyleValues.Double);
            lastDataRow = row;
        }

        var table = sheet.Range(headerRow, 1, Math.Max(headerRow, lastDataRow), Columns.Count);
        table.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        table.Style.Border.SetOutsideBorderColor(XLColor.FromHtml("#A6A6A6")).Border.SetInsideBorderColor(XLColor.FromHtml("#D9D9D9"));

        sheet.Columns(1, Columns.Count).AdjustToContents(headerRow, Math.Max(headerRow, lastDataRow));
        sheet.SheetView.FreezeRows(headerRow);
        sheet.PageSetup.PageOrientation = Landscape ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
        sheet.PageSetup.FitToPages(1, 0);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void WriteExcelRow(IXLWorksheet sheet, int row, object?[] values)
    {
        for (var c = 0; c < Columns.Count && c < values.Length; c++)
        {
            var cell = sheet.Cell(row, c + 1);
            switch (values[c])
            {
                case null:
                    break;
                case decimal d:
                    cell.Value = d;
                    cell.Style.NumberFormat.Format = Columns[c].Kind == ReportColumnKind.Money ? "#,##0.00" : "#,##0.####";
                    break;
                case DateTime dt:
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    break;
                default:
                    cell.SetValue(values[c]!.ToString());
                    break;
            }
        }
    }

    // ---------------- PDF ----------------

    public byte[] ToPdf(byte[]? logo) => new TableReportPdf(this, logo).GeneratePdf();

    private sealed class TableReportPdf : IDocument
    {
        private readonly InventoryTableReport _report;
        private readonly byte[]? _logo;

        public TableReportPdf(InventoryTableReport report, byte[]? logo)
        {
            _report = report;
            _logo = logo;
        }

        public DocumentMetadata GetMetadata() => new() { Title = _report.Title };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(_report.Landscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));
                page.Header().Element(ComposeHeader);
                page.Content().PaddingTop(8).Element(ComposeTable);
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Generado el {InventoryPostingService.NowLima():dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (_logo is not null)
                    {
                        row.ConstantItem(50).Height(40).Image(_logo).FitArea();
                        row.ConstantItem(10);
                    }
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(_report.Title).FontSize(14).Bold().FontColor(HeaderBg);
                        foreach (var subtitle in _report.Subtitles)
                        {
                            c.Item().Text(subtitle).FontSize(8);
                        }
                    });
                });
                col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
            });
        }

        private void ComposeTable(IContainer container)
        {
            var columns = _report.Columns;
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    foreach (var column in columns) c.RelativeColumn(column.Width);
                });

                table.Header(header =>
                {
                    foreach (var column in columns)
                    {
                        var cell = header.Cell().Background(HeaderBg).Padding(3);
                        (column.Kind == ReportColumnKind.Text ? cell.AlignLeft() : cell.AlignRight())
                            .Text(column.Header).FontColor(Colors.White).Bold().FontSize(7.5f);
                    }
                });

                foreach (var values in _report.Rows)
                {
                    WriteRow(table, values, bold: false);
                }

                if (_report.Totals is not null)
                {
                    WriteRow(table, _report.Totals, bold: true);
                }
            });
        }

        private void WriteRow(TableDescriptor table, object?[] values, bool bold)
        {
            for (var c = 0; c < _report.Columns.Count; c++)
            {
                var column = _report.Columns[c];
                var value = c < values.Length ? values[c] : null;
                var text = value switch
                {
                    null => "",
                    decimal d when column.Kind == ReportColumnKind.Money => d.ToString("N2"),
                    decimal d => d.ToString("#,##0.####"),
                    DateTime dt => dt.ToString("dd/MM/yyyy HH:mm"),
                    _ => value.ToString() ?? ""
                };

                var cell = table.Cell()
                    .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                    .Background(bold ? Colors.Grey.Lighten3 : Colors.White)
                    .PaddingVertical(2).PaddingHorizontal(3);
                var aligned = column.Kind == ReportColumnKind.Text ? cell.AlignLeft() : cell.AlignRight();
                var span = aligned.Text(text);
                if (bold) span.Bold();
            }
        }
    }
}

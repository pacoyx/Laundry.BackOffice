using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

/// <summary>
/// Planilla de remuneraciones del periodo en A4 horizontal: una fila por trabajador con ingresos,
/// descuentos por concepto, neto y espacio para firma.
/// </summary>
public class PayrollPdfDocument : IDocument
{
    private const string HeaderBg = "#1F4E78";
    private const string IncomeBg = "#E2EFDA";
    private const string DiscountBg = "#FCE4D6";
    private const string NetBg = "#DDEBF7";

    private readonly PayrollExportData _data;
    private readonly byte[]? _logo;
    private readonly DateTime _generatedAt = DateTime.Now;

    public PayrollPdfDocument(PayrollExportData data, byte[]? logo)
    {
        _data = data;
        _logo = logo;
    }

    public DocumentMetadata GetMetadata() => new() { Title = $"Planilla {_data.PeriodName}" };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(22);
            page.DefaultTextStyle(x => x.FontSize(7.5f));

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
                if (_logo is not null)
                {
                    row.ConstantItem(55).Height(45).Image(_logo).FitArea();
                    row.ConstantItem(10);
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_data.CompanyName).FontSize(11).Bold();
                    if (!string.IsNullOrWhiteSpace(_data.CompanyDocument))
                    {
                        col.Item().Text(_data.CompanyDocument).FontSize(8);
                    }
                });

                row.RelativeItem(2).AlignCenter().Column(col =>
                {
                    col.Item().AlignCenter().Text("PLANILLA DE REMUNERACIONES").FontSize(14).Bold().FontColor(HeaderBg);
                    col.Item().AlignCenter().Text(_data.PeriodName).FontSize(10).SemiBold();
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(t => { t.Span("Frecuencia: ").Bold(); t.Span(_data.FrequencyLabel); });
                    col.Item().AlignRight().Text(t =>
                    {
                        t.Span("Periodo: ").Bold();
                        t.Span($"{_data.StartDate:dd/MM/yyyy} - {_data.EndDate:dd/MM/yyyy}");
                    });
                    col.Item().AlignRight().Text(t => { t.Span("Estado: ").Bold(); t.Span(_data.IsClosed ? "Cerrado" : "Abierto"); });
                });
            });

            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(18);   // N°
                    c.RelativeColumn(3.2f); // Trabajador
                    c.RelativeColumn(1.8f); // Cargo
                    c.RelativeColumn(1.2f); // Sueldo periodo
                    c.RelativeColumn(1f);   // Horas extra
                    c.RelativeColumn(1f);   // Bonos
                    c.RelativeColumn(1.2f); // Total ingresos
                    c.RelativeColumn(1f);   // Faltas
                    c.RelativeColumn(1f);   // Tardanzas
                    c.RelativeColumn(1f);   // Préstamos
                    c.RelativeColumn(1f);   // Adelantos
                    c.RelativeColumn(1f);   // Otros
                    c.RelativeColumn(1.2f); // Total descuentos
                    c.RelativeColumn(1.3f); // Neto
                    c.RelativeColumn(2f);   // Firma
                });

                table.Header(header =>
                {
                    // Fila de grupos
                    header.Cell().ColumnSpan(3).Element(c => GroupCell(c, Colors.Grey.Lighten3)).Text("TRABAJADOR");
                    header.Cell().ColumnSpan(4).Element(c => GroupCell(c, IncomeBg)).Text("INGRESOS");
                    header.Cell().ColumnSpan(6).Element(c => GroupCell(c, DiscountBg)).Text("DESCUENTOS");
                    header.Cell().Element(c => GroupCell(c, NetBg)).Text("NETO");
                    header.Cell().Element(c => GroupCell(c, Colors.Grey.Lighten3)).Text("");

                    // Títulos
                    HeaderText(header, "N°");
                    HeaderText(header, "Apellidos y Nombres / DNI", left: true);
                    HeaderText(header, "Cargo", left: true);
                    HeaderText(header, "Sueldo Periodo");
                    HeaderText(header, "Horas Extras");
                    HeaderText(header, "Bonos");
                    HeaderText(header, "Total Ingresos");
                    HeaderText(header, "Faltas");
                    HeaderText(header, "Tardanzas");
                    HeaderText(header, "Préstamos");
                    HeaderText(header, "Adelantos");
                    HeaderText(header, "Otros");
                    HeaderText(header, "Total Desc.");
                    HeaderText(header, "Neto a Pagar");
                    HeaderText(header, "Firma");
                });

                var index = 1;
                foreach (var r in _data.Rows)
                {
                    table.Cell().Element(Cell).AlignCenter().Text(index++.ToString());
                    table.Cell().Element(Cell).Column(col =>
                    {
                        col.Item().Text(r.EmployeeName).SemiBold();
                        col.Item().Text($"DNI {r.DocumentNumber}").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Element(Cell).Text(r.PositionName);
                    MoneyCell(table, r.BaseAmount);
                    MoneyCell(table, r.ExtraHours);
                    MoneyCell(table, r.Bonus);
                    MoneyCell(table, r.GrossAmount, bold: true);
                    MoneyCell(table, r.AbsenceDiscount);
                    MoneyCell(table, r.LateArrivalDiscount);
                    MoneyCell(table, r.LoanDiscount);
                    MoneyCell(table, r.AdvanceDiscount);
                    MoneyCell(table, r.OtherDiscount);
                    MoneyCell(table, r.TotalDiscounts, bold: true);
                    table.Cell().Background(NetBg).Element(Cell).AlignRight().Text(r.NetAmount.ToString("N2")).Bold();
                    table.Cell().Element(Cell).Height(24).Text(""); // espacio para firma
                }

                // Totales
                table.Cell().ColumnSpan(3).Element(TotalCell).AlignRight().Text($"TOTALES ({_data.Rows.Count} trabajador(es))");
                TotalMoney(table, _data.Sum(x => x.BaseAmount));
                TotalMoney(table, _data.Sum(x => x.ExtraHours));
                TotalMoney(table, _data.Sum(x => x.Bonus));
                TotalMoney(table, _data.Sum(x => x.GrossAmount));
                TotalMoney(table, _data.Sum(x => x.AbsenceDiscount));
                TotalMoney(table, _data.Sum(x => x.LateArrivalDiscount));
                TotalMoney(table, _data.Sum(x => x.LoanDiscount));
                TotalMoney(table, _data.Sum(x => x.AdvanceDiscount));
                TotalMoney(table, _data.Sum(x => x.OtherDiscount));
                TotalMoney(table, _data.Sum(x => x.TotalDiscounts));
                TotalMoney(table, _data.Sum(x => x.NetAmount));
                table.Cell().Element(TotalCell).Text("");
            });

            // Firmas de responsables
            column.Item().PaddingTop(45).Row(row =>
            {
                row.RelativeItem();
                row.RelativeItem(2).Element(c => SignatureLine(c, "Elaborado por"));
                row.RelativeItem();
                row.RelativeItem(2).Element(c => SignatureLine(c, "Aprobado por"));
                row.RelativeItem();
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text($"Generado el {_generatedAt:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().AlignRight().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });
    }

    private static void SignatureLine(IContainer container, string label)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.8f).LineColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(3).AlignCenter().Text(label).FontSize(8);
        });
    }

    private static IContainer GroupCell(IContainer c, string background) =>
        c.Background(background).Border(0.5f).BorderColor(Colors.Grey.Lighten1)
         .PaddingVertical(2).AlignCenter().DefaultTextStyle(x => x.Bold().FontSize(7));

    private static void HeaderText(TableCellDescriptor header, string text, bool left = false)
    {
        var cell = header.Cell()
            .Background(HeaderBg).Border(0.5f).BorderColor(Colors.Grey.Lighten1)
            .PaddingVertical(3).PaddingHorizontal(3).AlignMiddle();
        (left ? cell.AlignLeft() : cell.AlignCenter())
            .Text(text).FontColor(Colors.White).Bold().FontSize(7);
    }

    private static IContainer Cell(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(3).AlignMiddle();

    private static void MoneyCell(TableDescriptor table, decimal value, bool bold = false)
    {
        var text = table.Cell().Element(Cell).AlignRight().Text(value == 0 ? "-" : value.ToString("N2"));
        if (bold) text.SemiBold();
    }

    private static IContainer TotalCell(IContainer c) =>
        c.Background(Colors.Grey.Lighten2).BorderTop(1.2f).BorderColor(Colors.Grey.Darken2)
         .PaddingVertical(3).PaddingHorizontal(3).DefaultTextStyle(x => x.Bold());

    private static void TotalMoney(TableDescriptor table, decimal value) =>
        table.Cell().Element(TotalCell).AlignRight().Text(value.ToString("N2"));
}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

/// <summary>
/// Ficha PDF de un activo fijo: foto, datos, valor en libros, indicadores y su historial completo.
/// </summary>
public class FixedAssetSheetPdfDocument : IDocument
{
    private readonly FixedAsset _asset;
    private readonly FixedAssetHistorySummaryDto _summary;
    private readonly List<FixedAssetHistoryEventDto> _events;
    private readonly byte[]? _photo;
    private readonly byte[]? _logo;
    private readonly string? _companyName;
    private readonly DateTime _generatedAt = DateTime.Now;

    public FixedAssetSheetPdfDocument(
        FixedAsset asset,
        FixedAssetHistorySummaryDto summary,
        List<FixedAssetHistoryEventDto> events,
        byte[]? photo,
        byte[]? logo,
        string? companyName)
    {
        _asset = asset;
        _summary = summary;
        _events = events;
        _photo = photo;
        _logo = logo;
        _companyName = companyName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Generado el {_generatedAt:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
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
                    row.ConstantItem(55).Height(55).Image(_logo).FitArea();
                    row.ConstantItem(10);
                }

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("FICHA DE ACTIVO FIJO").FontSize(15).Bold();
                    if (!string.IsNullOrWhiteSpace(_companyName))
                    {
                        col.Item().Text(_companyName).FontSize(9).FontColor(Colors.Grey.Darken2);
                    }
                });

                row.ConstantItem(140).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(_asset.Code).FontSize(14).Bold();
                    col.Item().AlignRight().Text(FixedAssetHistoryReportBuilder.StatusLabel(_asset.Status))
                        .FontColor(StatusColor(_asset.Status)).Bold();
                });
            });
            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Spacing(12);

            // Foto + datos
            column.Item().Row(row =>
            {
                row.ConstantItem(170).Height(130).Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                    .AlignCenter().AlignMiddle().Element(c =>
                    {
                        if (_photo is not null) c.Image(_photo).FitArea();
                        else c.Text("Sin foto").FontColor(Colors.Grey.Medium);
                    });

                row.ConstantItem(14);

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_asset.Name).FontSize(13).Bold();
                    if (!string.IsNullOrWhiteSpace(_asset.Description))
                    {
                        col.Item().PaddingBottom(4).Text(_asset.Description).FontColor(Colors.Grey.Darken2);
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.ConstantColumn(105); c.RelativeColumn(); });
                        void Dato(string label, string? value)
                        {
                            table.Cell().PaddingVertical(1.5f).Text(label).Bold();
                            table.Cell().PaddingVertical(1.5f).Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
                        }

                        Dato("Categoría", _asset.Category.Name);
                        Dato("Sucursal", _asset.Branch.Description);
                        Dato("Responsable", _asset.ResponsibleUser?.Name);
                        Dato("Fecha adquisición", _asset.AcquisitionDate.ToString("dd/MM/yyyy"));
                        Dato("Proveedor", _asset.Supplier?.Name);
                        Dato("Factura de compra", _asset.PurchaseInvoice is null ? null
                            : $"{_asset.PurchaseInvoice.InvoiceSerie}-{_asset.PurchaseInvoice.InvoiceNumber}");
                        Dato("Vida útil", $"{FixedAssetDepreciationCalculator.GetEffectiveUsefulLifeYears(_asset)} años");
                        if (_asset.DisposalDate.HasValue)
                        {
                            Dato(_asset.Status == FixedAssetStatus.Sold ? "Fecha de venta" : "Fecha de baja",
                                $"{_asset.DisposalDate:dd/MM/yyyy} — {_asset.DisposalReason}");
                        }
                    });
                });
            });

            // Indicadores
            column.Item().Row(row =>
            {
                row.Spacing(8);
                Indicador(row, "Costo de adquisición", _summary.AcquisitionCost.ToString("N2"));
                Indicador(row, "Valor en libros", _summary.BookValue.ToString("N2"));
                Indicador(row, "Mantenimientos", $"{_summary.MaintenanceCount}  ({_summary.MaintenanceCost:N2})");
                Indicador(row, "Días fuera de servicio", _summary.DaysOutOfService.ToString());
                Indicador(row, "Próximo mantenimiento", _summary.NextMaintenanceDate?.ToString("dd/MM/yyyy") ?? "-");
            });

            if (!string.IsNullOrWhiteSpace(_asset.Notes))
            {
                column.Item().Text(text =>
                {
                    text.Span("Notas: ").Bold();
                    text.Span(_asset.Notes);
                });
            }

            // Historial
            column.Item().Text("Historial").FontSize(12).Bold();
            column.Item().Element(ComposeHistory);
        });
    }

    private static void Indicador(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(6).Column(col =>
        {
            col.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken2);
            col.Item().Text(value).FontSize(11).Bold();
        });
    }

    private void ComposeHistory(IContainer container)
    {
        if (_events.Count == 0)
        {
            container.Text("Sin eventos registrados.").FontColor(Colors.Grey.Medium);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(58);
                c.ConstantColumn(95);
                c.RelativeColumn();
                c.ConstantColumn(60);
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Fecha");
                h.Cell().Element(HeaderCell).Text("Evento");
                h.Cell().Element(HeaderCell).Text("Descripción / detalle");
                h.Cell().Element(HeaderCell).AlignRight().Text("Costo");

                static IContainer HeaderCell(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White)).Background(Colors.Blue.Darken2).Padding(4);
            });

            foreach (var e in _events)
            {
                table.Cell().Element(Cell).Text(e.Date.ToString("dd/MM/yyyy"));
                table.Cell().Element(Cell).Text(e.EventLabel).Bold()
                    .FontColor(e.InProgress ? Colors.Orange.Darken3 : Colors.Black);
                table.Cell().Element(Cell).Column(col =>
                {
                    col.Item().Text(e.Description);
                    if (!string.IsNullOrWhiteSpace(e.Detail))
                    {
                        col.Item().Text(e.Detail).FontSize(8).FontColor(Colors.Grey.Darken2);
                    }
                });
                table.Cell().Element(Cell).AlignRight().Text(e.Cost?.ToString("N2") ?? "");
            }

            static IContainer Cell(IContainer c) =>
                c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        });
    }

    private static string StatusColor(FixedAssetStatus status) => status switch
    {
        FixedAssetStatus.Active => Colors.Green.Darken2,
        FixedAssetStatus.InMaintenance => Colors.Orange.Darken3,
        _ => Colors.Red.Darken2
    };
}

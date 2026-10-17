using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ValuationReportPhotosPdfDocument : IDocument
{
    private const string LogoPath = "wwwroot/images/logo.png";
    private const int PhotosPerPage = 2;

    private readonly List<ValuationReportPhotoItem> _photos;
    private readonly string _customerName;
    private readonly ValuationReportFilter _filter;
    private readonly DateTime _generatedAt;

    public ValuationReportPhotosPdfDocument(List<ValuationReportPhotoItem> photos, string customerName, ValuationReportFilter filter)
    {
        _photos = photos;
        _customerName = customerName;
        _filter = filter;
        _generatedAt = DateTime.Now;
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
                    col.Item().AlignCenter().Text("VALORIZACIÓN DEL MES - EVIDENCIA FOTOGRÁFICA DE GUÍAS").FontSize(14).Bold();
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
        container.Column(column =>
        {
            if (_photos.Count == 0)
            {
                column.Item().PaddingTop(40).AlignCenter()
                    .Text("No se encontraron fotos de guía física en el periodo seleccionado.").Italic();
                return;
            }

            for (var i = 0; i < _photos.Count; i++)
            {
                var photo = _photos[i];

                column.Item().PaddingBottom(10).Column(photoColumn =>
                {
                    photoColumn.Item().Text($"Guía: {photo.GuideNumber}    Fecha: {photo.PickupDate:dd/MM/yyyy}").Bold().FontSize(10);
                    photoColumn.Item().PaddingTop(4).Height(330).Image(photo.ImageBytes).FitArea();
                });

                var isLastOfPage = (i + 1) % PhotosPerPage == 0;
                var isLastOverall = i == _photos.Count - 1;
                if (isLastOfPage && !isLastOverall)
                {
                    column.Item().PageBreak();
                }
            }
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

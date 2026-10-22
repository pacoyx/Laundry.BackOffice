using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PickupGuideReceiptPdfDocument : IDocument
{
    private readonly PickupGuide _guide;
    private readonly string _companyName;
    private readonly byte[]? _logoImage;
    private readonly byte[]? _signatureImage;
    private readonly List<byte[]> _photoImages;
    private readonly List<PickupGuideRevisionPdfData> _revisions;
    private readonly Dictionary<long, decimal> _originalQuantityByDetailId;
    private readonly HashSet<long> _addedItemDetailIds;
    private readonly DateTime _generatedAt;

    public PickupGuideReceiptPdfDocument(
        PickupGuide guide,
        string companyName,
        byte[]? logoImage,
        byte[]? signatureImage,
        List<byte[]> photoImages,
        List<PickupGuideRevisionPdfData> revisions,
        Dictionary<long, decimal> originalQuantityByDetailId,
        HashSet<long> addedItemDetailIds)
    {
        _guide = guide;
        _companyName = companyName;
        _logoImage = logoImage;
        _signatureImage = signatureImage;
        _photoImages = photoImages;
        _revisions = revisions;
        _originalQuantityByDetailId = originalQuantityByDetailId;
        _addedItemDetailIds = addedItemDetailIds;
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
                if (_logoImage is not null)
                {
                    row.ConstantItem(60).Height(60).Image(_logoImage).FitArea();
                }

                row.RelativeItem().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(_companyName))
                    {
                        col.Item().AlignCenter().Text(_companyName).FontSize(13).Bold();
                    }

                    col.Item().AlignCenter().Text("CONSTANCIA DE RECOJO").FontSize(16).Bold();

                    if (!string.IsNullOrWhiteSpace(_guide.PhysicalGuideNumber))
                    {
                        col.Item().AlignCenter().Text($"Guía: {_guide.PhysicalGuideNumber}").FontSize(14).Bold();
                        col.Item().AlignCenter().Text($"{_guide.GuideNumber} (Interno)").FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
                    }
                    else
                    {
                        col.Item().AlignCenter().Text($"Guía: {_guide.GuideNumber}").FontSize(11).Bold();
                    }

                    col.Item().AlignCenter().Text($"Generado el: {_generatedAt:dd/MM/yyyy HH:mm}").FontSize(8);
                });
            });

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Cliente: ").Bold();
                    text.Span(PickupGuidesEndpoints.GetCustomerDisplayName(_guide.Customer));
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Fecha de recojo: ").Bold();
                    text.Span(_guide.PickupDate.ToString("dd/MM/yyyy"));
                });
            });

            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Empleado: ").Bold();
                    text.Span(_guide.Employee is null ? "-" : $"{_guide.Employee.FirstName} {_guide.Employee.LastName}".Trim());
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Vehículo: ").Bold();
                    text.Span(_guide.Vehicle?.Plate ?? "-");
                });
            });

            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Estado: ").Bold();
                    text.Span(_guide.Status.ToString());
                });
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Element(ComposeInitialPickupSection);

            if (_revisions.Count > 0)
            {
                column.Item().PaddingTop(15).Element(ComposeRevisionsSection);
            }

            column.Item().PaddingTop(15).Element(ComposeConfirmedQuantitiesSection);

            if (!string.IsNullOrWhiteSpace(_guide.Observation))
            {
                column.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Observaciones: ").Bold();
                    text.Span(_guide.Observation);
                });
            }

            column.Item().PaddingTop(15).Element(ComposePickupEvidenceSection);

            if (_revisions.Any(r => r.PhotoImages.Count > 0))
            {
                column.Item().PaddingTop(15).Element(ComposeRevisionEvidenceSection);
            }

            if (_revisions.Count > 0)
            {
                column.Item().PaddingTop(15).Element(ComposeTraceabilitySummarySection);
            }
        });
    }

    private void ComposeInitialPickupSection(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("REGISTRO INICIAL DE RECOJO").Bold().FontSize(11);

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.4f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Servicio");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cantidad Declarada");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Peso Declarado");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Importe");
                });

                // Los ítems agregados durante una revisión no formaban parte del recojo inicial;
                // ese cambio ya queda documentado en "REVISIONES REGISTRADAS".
                foreach (var detail in _guide.Details.Where(d => !_addedItemDetailIds.Contains(d.Id)))
                {
                    var isPricedByWeight = detail.ServiceItem.IsPeso == "S";
                    var originalQuantity = _originalQuantityByDetailId.TryGetValue(detail.Id, out var value)
                        ? value
                        : (isPricedByWeight ? detail.Weight ?? 0 : detail.Quantity);
                    var originalAmount = originalQuantity * detail.UnitPrice;

                    table.Cell().Element(CellStyle).Text(detail.ServiceItem.Name);
                    table.Cell().Element(CellStyle).AlignRight().Text(isPricedByWeight ? "-" : originalQuantity.ToString("N2"));
                    table.Cell().Element(CellStyle).AlignRight().Text(isPricedByWeight ? $"{originalQuantity:N2} kg" : "-");
                    table.Cell().Element(CellStyle).AlignRight().Text(originalAmount.ToString("N2"));
                }
            });
        });
    }

    private void ComposeRevisionsSection(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("REVISIONES REGISTRADAS").Bold().FontSize(11);

            for (var i = 0; i < _revisions.Count; i++)
            {
                var revision = _revisions[i].Revision;

                column.Item().PaddingTop(8).Background(Colors.Grey.Lighten4).Padding(6).Column(revColumn =>
                {
                    revColumn.Item().Text($"Revisión #{i + 1}").Bold();
                    revColumn.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Fecha: ").Bold();
                        text.Span(revision.RevisionDate.ToString("dd/MM/yyyy"));
                    });
                    revColumn.Item().Text(text =>
                    {
                        text.Span("Empleado: ").Bold();
                        text.Span($"{revision.Employee.FirstName} {revision.Employee.LastName}".Trim());
                    });

                    if (!string.IsNullOrWhiteSpace(revision.Observation))
                    {
                        revColumn.Item().PaddingTop(2).Text(text =>
                        {
                            text.Span("Observación: ").Bold();
                            text.Span(revision.Observation);
                        });
                    }

                    if (revision.DetailRevisions.Count > 0)
                    {
                        revColumn.Item().PaddingTop(6).Text("Cambios detectados:").Bold().FontSize(8.5f);

                        revColumn.Item().PaddingTop(3).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(3.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("Servicio");
                                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Original → Corregido");
                                header.Cell().Element(HeaderCellStyle).Text("Motivo");
                            });

                            foreach (var detailRevision in revision.DetailRevisions)
                            {
                                var isPricedByWeight = detailRevision.PickupGuideDetail.ServiceItem.IsPeso == "S";
                                var suffix = isPricedByWeight ? " kg" : "";

                                table.Cell().Element(CellStyle).Text(detailRevision.PickupGuideDetail.ServiceItem.Name);
                                table.Cell().Element(CellStyle).AlignCenter()
                                    .Text($"{detailRevision.OriginalQuantity:N2}{suffix} → {detailRevision.NewQuantity:N2}{suffix}");
                                table.Cell().Element(CellStyle).Text(detailRevision.Reason);
                            }
                        });
                    }
                });
            }
        });
    }

    private void ComposeConfirmedQuantitiesSection(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("CANTIDADES CONFIRMADAS").Bold().FontSize(11);

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.4f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Servicio");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cantidad Final");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Peso Final");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Importe Final");
                });

                foreach (var detail in _guide.Details)
                {
                    var isPricedByWeight = detail.ServiceItem.IsPeso == "S";

                    table.Cell().Element(CellStyle).Text(detail.ServiceItem.Name);
                    table.Cell().Element(CellStyle).AlignRight().Text(isPricedByWeight ? "-" : detail.Quantity.ToString("N2"));
                    table.Cell().Element(CellStyle).AlignRight().Text(isPricedByWeight ? $"{detail.Weight:N2} kg" : "-");
                    table.Cell().Element(CellStyle).AlignRight().Text(detail.Total.ToString("N2"));
                }
            });

            column.Item().PaddingTop(8).AlignRight().Text(text =>
            {
                text.Span("Total: ").Bold();
                text.Span(_guide.TotalAmount.ToString("N2"));
            });
        });
    }

    private void ComposePickupEvidenceSection(IContainer container)
    {
        container.Column(column =>
        {
            if (_photoImages.Count == 0 && _signatureImage is null)
            {
                return;
            }

            column.Item().Text("EVIDENCIAS DE RECOJO").Bold().FontSize(11);

            if (_photoImages.Count > 0)
            {
                column.Item().PaddingTop(8).Text("Fotos de recojo").Bold().FontSize(9);
                column.Item().PaddingTop(5).Row(row =>
                {
                    foreach (var photo in _photoImages)
                    {
                        row.RelativeItem().Padding(3).Height(140).Image(photo).FitArea();
                    }
                });
            }

            if (_signatureImage is not null)
            {
                column.Item().PaddingTop(10).Text("Firma de conformidad").Bold().FontSize(9);
                column.Item().PaddingTop(5).Height(100).Width(200).Image(_signatureImage).FitArea();
            }
        });
    }

    private void ComposeRevisionEvidenceSection(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("EVIDENCIAS DE REVISIÓN").Bold().FontSize(11);

            for (var i = 0; i < _revisions.Count; i++)
            {
                var revisionData = _revisions[i];
                if (revisionData.PhotoImages.Count == 0)
                {
                    continue;
                }

                column.Item().PaddingTop(8).Text($"Evidencias Revisión #{i + 1}").Bold().FontSize(9);

                if (!string.IsNullOrWhiteSpace(revisionData.Revision.Observation))
                {
                    column.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Observación: ").Bold();
                        text.Span(revisionData.Revision.Observation);
                    });
                }

                column.Item().PaddingTop(5).Row(row =>
                {
                    foreach (var photo in revisionData.PhotoImages)
                    {
                        row.RelativeItem().Padding(3).Height(140).Image(photo).FitArea();
                    }
                });
            }
        });
    }

    private void ComposeTraceabilitySummarySection(IContainer container)
    {
        var lastRevision = _revisions[^1].Revision;

        container.Column(column =>
        {
            column.Item().Text("RESUMEN DE TRAZABILIDAD").Bold().FontSize(11);

            column.Item().PaddingTop(5).Text(text =>
            {
                text.Span("Revisiones realizadas: ").Bold();
                text.Span(_revisions.Count.ToString());
            });
            column.Item().Text(text =>
            {
                text.Span("Última revisión: ").Bold();
                text.Span(lastRevision.RevisionDate.ToString("dd/MM/yyyy"));
            });
            column.Item().Text(text =>
            {
                text.Span("Último revisor: ").Bold();
                text.Span($"{lastRevision.Employee.FirstName} {lastRevision.Employee.LastName}".Trim());
            });
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

    private static IContainer HeaderCellStyle(IContainer c) =>
        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
         .Background(Colors.Blue.Darken2)
         .Padding(4);

    private static IContainer CellStyle(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
}

using ClosedXML.Excel;

public static class FixedAssetHistoryExcelBuilder
{
    private static readonly XLColor HeaderColor = XLColor.FromHtml("#BDD7EE");

    public static byte[] Build(FixedAssetHistoryReportResponse report, FixedAssetHistoryReportFilter filter)
    {
        using var workbook = new XLWorkbook();
        BuildSummarySheet(workbook.Worksheets.Add("Resumen por activo"), report, filter);
        BuildEventsSheet(workbook.Worksheets.Add("Historial de eventos"), report);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void BuildSummarySheet(IXLWorksheet sheet, FixedAssetHistoryReportResponse report, FixedAssetHistoryReportFilter filter)
    {
        sheet.Cell(1, 1).Value = "Reporte de historial de activos fijos";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        var periodo = (filter.FromDate, filter.ToDate) switch
        {
            (null, null) => "todo el historial",
            ({ } desde, null) => $"desde el {desde:dd/MM/yyyy}",
            (null, { } hasta) => $"hasta el {hasta:dd/MM/yyyy}",
            ({ } desde, { } hasta) => $"del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}",
        };
        sheet.Cell(2, 1).Value = $"Periodo: {periodo}   ·   Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        string[] headers =
        {
            "Código", "Activo", "Categoría", "Sucursal", "Responsable", "Estado", "Fecha adquisición",
            "Costo adquisición", "Valor en libros", "N° mantenimientos", "Costo mantenimientos",
            "N° paradas", "Días fuera de servicio", "N° reasignaciones", "Último mantenimiento", "Próximo mantenimiento"
        };
        const int headerRow = 4;
        WriteHeader(sheet, headerRow, headers);

        var row = headerRow + 1;
        foreach (var a in report.Assets)
        {
            sheet.Cell(row, 1).Value = a.Code;
            sheet.Cell(row, 2).Value = a.Name;
            sheet.Cell(row, 3).Value = a.CategoryName;
            sheet.Cell(row, 4).Value = a.BranchName;
            sheet.Cell(row, 5).Value = a.ResponsibleUserName ?? "-";
            sheet.Cell(row, 6).Value = FixedAssetHistoryReportBuilder.StatusLabel(a.Status);
            SetDate(sheet.Cell(row, 7), a.AcquisitionDate);
            SetMoney(sheet.Cell(row, 8), a.AcquisitionCost);
            SetMoney(sheet.Cell(row, 9), a.BookValue);
            sheet.Cell(row, 10).Value = a.MaintenanceCount;
            SetMoney(sheet.Cell(row, 11), a.MaintenanceCost);
            sheet.Cell(row, 12).Value = a.OutOfServiceCount;
            sheet.Cell(row, 13).Value = a.DaysOutOfService;
            sheet.Cell(row, 14).Value = a.ReassignmentCount;
            SetDate(sheet.Cell(row, 15), a.LastMaintenanceDate);
            SetDate(sheet.Cell(row, 16), a.NextMaintenanceDate);
            row++;
        }

        // Totales
        var t = report.Totals;
        sheet.Cell(row, 1).Value = $"TOTAL ({t.AssetCount} activos)";
        SetMoney(sheet.Cell(row, 8), t.TotalAcquisitionCost);
        SetMoney(sheet.Cell(row, 9), t.TotalBookValue);
        sheet.Cell(row, 10).Value = t.MaintenanceCount;
        SetMoney(sheet.Cell(row, 11), t.TotalMaintenanceCost);
        sheet.Cell(row, 13).Value = t.TotalDaysOutOfService;
        sheet.Row(row).Style.Font.Bold = true;

        if (report.Assets.Count > 0)
        {
            sheet.Range(headerRow, 1, row - 1, headers.Length).SetAutoFilter();
        }
        sheet.SheetView.FreezeRows(headerRow);
        sheet.Columns().AdjustToContents();
    }

    private static void BuildEventsSheet(IXLWorksheet sheet, FixedAssetHistoryReportResponse report)
    {
        string[] headers =
        {
            "Fecha", "Código", "Activo", "Categoría", "Sucursal", "Evento", "Descripción", "Detalle",
            "Costo", "Fecha fin", "Días fuera de servicio", "En curso"
        };
        WriteHeader(sheet, 1, headers);

        var row = 2;
        foreach (var e in report.Events)
        {
            SetDate(sheet.Cell(row, 1), e.Date);
            sheet.Cell(row, 2).Value = e.AssetCode;
            sheet.Cell(row, 3).Value = e.AssetName;
            sheet.Cell(row, 4).Value = e.CategoryName;
            sheet.Cell(row, 5).Value = e.BranchName;
            sheet.Cell(row, 6).Value = e.EventLabel;
            sheet.Cell(row, 7).Value = e.Description;
            sheet.Cell(row, 8).Value = e.Detail ?? "";
            if (e.Cost.HasValue) SetMoney(sheet.Cell(row, 9), e.Cost.Value);
            SetDate(sheet.Cell(row, 10), e.EndDate);
            if (e.DaysOutOfService.HasValue) sheet.Cell(row, 11).Value = e.DaysOutOfService.Value;
            sheet.Cell(row, 12).Value = e.InProgress ? "Sí" : "";
            row++;
        }

        if (report.Events.Count > 0)
        {
            sheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        // El detalle puede ser largo: ancho fijo con ajuste de texto
        sheet.Column(8).Width = 70;
        sheet.Column(8).Style.Alignment.WrapText = true;
    }

    private static void WriteHeader(IXLWorksheet sheet, int row, string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = HeaderColor;
        }
    }

    private static void SetDate(IXLCell cell, DateTime? date)
    {
        if (!date.HasValue) return;
        cell.Value = date.Value;
        cell.Style.DateFormat.Format = "dd/MM/yyyy";
    }

    private static void SetMoney(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.NumberFormat.Format = "#,##0.00";
    }
}

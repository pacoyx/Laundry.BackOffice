using ClosedXML.Excel;

public static class ValuationReportSummaryExcelBuilder
{
    public static byte[] Build(ValuationReportPivotDto pivot, string customerName, ValuationReportFilter filter)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Valorizacion");

        sheet.Cell(1, 1).Value = "Cliente:";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 2).Value = customerName;

        sheet.Cell(2, 1).Value = "Periodo:";
        sheet.Cell(2, 1).Style.Font.Bold = true;
        sheet.Cell(2, 2).Value = $"{filter.FromDate:dd/MM/yyyy} - {filter.ToDate:dd/MM/yyyy}";

        const int headerRow = 4;
        WriteHeader(sheet, pivot, headerRow);
        var lastDataRow = WriteRows(sheet, pivot, headerRow + 1);
        WriteTotalsRow(sheet, pivot, lastDataRow + 1);

        sheet.SheetView.FreezeRows(headerRow);
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteHeader(IXLWorksheet sheet, ValuationReportPivotDto pivot, int row)
    {
        var dateCell = sheet.Cell(row, 1);
        dateCell.Value = "Fecha";
        dateCell.Style.Font.Bold = true;
        dateCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");

        var column = 2;
        foreach (var service in pivot.Services)
        {
            var cell = sheet.Cell(row, column);
            cell.Value = service.ServiceName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
            column++;
        }

        var totalCell = sheet.Cell(row, column);
        totalCell.Value = "Total";
        totalCell.Style.Font.Bold = true;
        totalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
    }

    private static int WriteRows(IXLWorksheet sheet, ValuationReportPivotDto pivot, int startRow)
    {
        var currentRow = startRow;

        foreach (var row in pivot.Rows)
        {
            sheet.Cell(currentRow, 1).Value = row.Date;
            sheet.Cell(currentRow, 1).Style.DateFormat.Format = "dd/MM/yyyy";

            var column = 2;
            foreach (var service in pivot.Services)
            {
                var value = row.QuantitiesByServiceItemId.GetValueOrDefault(service.ServiceItemId);
                var cell = sheet.Cell(currentRow, column);
                cell.Value = value;
                cell.Style.NumberFormat.Format = "#,##0.00";
                column++;
            }

            var totalCell = sheet.Cell(currentRow, column);
            totalCell.Value = row.RowTotal;
            totalCell.Style.NumberFormat.Format = "#,##0.00";
            totalCell.Style.Font.Bold = true;

            currentRow++;
        }

        return currentRow - 1;
    }

    private static void WriteTotalsRow(IXLWorksheet sheet, ValuationReportPivotDto pivot, int row)
    {
        var labelCell = sheet.Cell(row, 1);
        labelCell.Value = "TOTAL";
        labelCell.Style.Font.Bold = true;
        labelCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");

        var column = 2;
        foreach (var service in pivot.Services)
        {
            var cell = sheet.Cell(row, column);
            cell.Value = pivot.ColumnTotals.GetValueOrDefault(service.ServiceItemId);
            cell.Style.NumberFormat.Format = "#,##0.00";
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
            column++;
        }

        var grandTotalCell = sheet.Cell(row, column);
        grandTotalCell.Value = pivot.GrandTotal;
        grandTotalCell.Style.NumberFormat.Format = "#,##0.00";
        grandTotalCell.Style.Font.Bold = true;
        grandTotalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
    }
}

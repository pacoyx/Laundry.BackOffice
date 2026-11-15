using ClosedXML.Excel;

public static class PayrollExcelBuilder
{
    private const string MoneyFormat = "#,##0.00";
    private static readonly XLColor HeaderColor = XLColor.FromHtml("#1F4E78");
    private static readonly XLColor IncomeColor = XLColor.FromHtml("#E2EFDA");
    private static readonly XLColor DiscountColor = XLColor.FromHtml("#FCE4D6");
    private static readonly XLColor NetColor = XLColor.FromHtml("#DDEBF7");
    private static readonly XLColor EmployeeColor = XLColor.FromHtml("#F2F2F2");

    // Columnas de la hoja principal
    private const int ColIndex = 1, ColCode = 2, ColDocument = 3, ColName = 4, ColArea = 5, ColPosition = 6, ColMonthly = 7;
    private const int ColBase = 8, ColExtra = 9, ColBonus = 10, ColGross = 11;
    private const int ColAbsence = 12, ColLate = 13, ColLoan = 14, ColAdvance = 15, ColOther = 16, ColTotalDiscounts = 17;
    private const int ColNet = 18;
    private const int LastCol = ColNet;

    public static byte[] Build(PayrollExportData data)
    {
        using var workbook = new XLWorkbook();
        BuildSummarySheet(workbook.Worksheets.Add("Planilla"), data);
        BuildDetailSheet(workbook.Worksheets.Add("Detalle"), data);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void BuildSummarySheet(IXLWorksheet sheet, PayrollExportData data)
    {
        // Encabezado del documento
        sheet.Cell(1, 1).Value = data.CompanyName;
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);
        sheet.Cell(2, 1).Value = data.CompanyDocument;
        sheet.Cell(3, 1).Value = "PLANILLA DE REMUNERACIONES";
        sheet.Cell(3, 1).Style.Font.SetBold().Font.SetFontSize(13).Font.SetFontColor(HeaderColor);
        sheet.Cell(4, 1).Value =
            $"Periodo: {data.PeriodName}   |   Frecuencia: {data.FrequencyLabel}   |   Del {data.StartDate:dd/MM/yyyy} al {data.EndDate:dd/MM/yyyy}   |   Estado: {(data.IsClosed ? "Cerrado" : "Abierto")}";
        sheet.Cell(5, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
        sheet.Cell(5, 1).Style.Font.SetItalic().Font.SetFontSize(9);

        // Fila de grupos
        const int groupRow = 7;
        Group(sheet, groupRow, ColIndex, ColMonthly, "DATOS DEL TRABAJADOR", EmployeeColor);
        Group(sheet, groupRow, ColBase, ColGross, "INGRESOS", IncomeColor);
        Group(sheet, groupRow, ColAbsence, ColTotalDiscounts, "DESCUENTOS", DiscountColor);
        Group(sheet, groupRow, ColNet, ColNet, "NETO", NetColor);

        // Fila de títulos
        const int headerRow = groupRow + 1;
        var headers = new Dictionary<int, string>
        {
            [ColIndex] = "N°", [ColCode] = "Código", [ColDocument] = "DNI", [ColName] = "Trabajador",
            [ColArea] = "Área", [ColPosition] = "Cargo", [ColMonthly] = "Sueldo Mensual",
            [ColBase] = "Sueldo Periodo", [ColExtra] = "Horas Extras", [ColBonus] = "Bonos", [ColGross] = "Total Ingresos",
            [ColAbsence] = "Faltas", [ColLate] = "Tardanzas", [ColLoan] = "Préstamos", [ColAdvance] = "Adelantos",
            [ColOther] = "Otros Desc.", [ColTotalDiscounts] = "Total Descuentos", [ColNet] = "Neto a Pagar"
        };
        foreach (var (col, title) in headers)
        {
            var cell = sheet.Cell(headerRow, col);
            cell.Value = title;
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            cell.Style.Fill.SetBackgroundColor(HeaderColor);
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Alignment.WrapText = true;
        }
        sheet.Row(headerRow).Height = 30;

        // Datos
        var row = headerRow + 1;
        var firstDataRow = row;
        var index = 1;
        foreach (var r in data.Rows)
        {
            sheet.Cell(row, ColIndex).Value = index++;
            sheet.Cell(row, ColCode).Value = r.EmployeeCode;
            sheet.Cell(row, ColDocument).SetValue(r.DocumentNumber); // texto: conserva ceros a la izquierda
            sheet.Cell(row, ColName).Value = r.EmployeeName;
            sheet.Cell(row, ColArea).Value = r.AreaName;
            sheet.Cell(row, ColPosition).Value = r.PositionName;
            Money(sheet.Cell(row, ColMonthly), r.MonthlySalary);
            Money(sheet.Cell(row, ColBase), r.BaseAmount);
            Money(sheet.Cell(row, ColExtra), r.ExtraHours);
            Money(sheet.Cell(row, ColBonus), r.Bonus);
            Money(sheet.Cell(row, ColGross), r.GrossAmount).Style.Font.SetBold();
            Money(sheet.Cell(row, ColAbsence), r.AbsenceDiscount);
            Money(sheet.Cell(row, ColLate), r.LateArrivalDiscount);
            Money(sheet.Cell(row, ColLoan), r.LoanDiscount);
            Money(sheet.Cell(row, ColAdvance), r.AdvanceDiscount);
            Money(sheet.Cell(row, ColOther), r.OtherDiscount);
            Money(sheet.Cell(row, ColTotalDiscounts), r.TotalDiscounts).Style.Font.SetBold();
            Money(sheet.Cell(row, ColNet), r.NetAmount).Style.Font.SetBold();
            sheet.Cell(row, ColNet).Style.Fill.SetBackgroundColor(NetColor);
            row++;
        }
        var lastDataRow = row - 1;

        // Totales
        sheet.Range(row, ColIndex, row, ColMonthly).Merge().Value = $"TOTALES ({data.Rows.Count} trabajador(es))";
        sheet.Cell(row, ColIndex).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        for (var col = ColBase; col <= LastCol; col++)
        {
            var letter = sheet.Column(col).ColumnLetter();
            var cell = sheet.Cell(row, col);
            cell.FormulaA1 = $"SUM({letter}{firstDataRow}:{letter}{lastDataRow})";
            cell.Style.NumberFormat.Format = MoneyFormat;
        }
        var totalsRange = sheet.Range(row, ColIndex, row, LastCol);
        totalsRange.Style.Font.SetBold();
        totalsRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9D9D9"));
        totalsRange.Style.Border.SetTopBorder(XLBorderStyleValues.Double);

        // Bordes y formato general
        var table = sheet.Range(groupRow, ColIndex, row, LastCol);
        table.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        table.Style.Border.SetOutsideBorderColor(XLColor.FromHtml("#A6A6A6")).Border.SetInsideBorderColor(XLColor.FromHtml("#D9D9D9"));

        sheet.Columns(ColIndex, LastCol).AdjustToContents(headerRow, row);
        sheet.Column(ColIndex).Width = 5;
        sheet.Column(ColName).Width = Math.Max(sheet.Column(ColName).Width, 28);
        for (var col = ColMonthly; col <= LastCol; col++)
        {
            sheet.Column(col).Width = Math.Max(sheet.Column(col).Width, 12);
        }

        sheet.SheetView.FreezeRows(headerRow);
        sheet.SheetView.FreezeColumns(ColName);

        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        sheet.PageSetup.FitToPages(1, 0);
        sheet.PageSetup.SetRowsToRepeatAtTop(groupRow, headerRow);
    }

    private static void BuildDetailSheet(IXLWorksheet sheet, PayrollExportData data)
    {
        sheet.Cell(1, 1).Value = $"DETALLE DE PLANILLA - {data.PeriodName}";
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(13).Font.SetFontColor(HeaderColor);

        var headers = new[] { "Trabajador", "DNI", "Concepto", "Descripción", "Monto" };
        const int headerRow = 3;
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White);
            cell.Style.Fill.SetBackgroundColor(HeaderColor);
        }

        var row = headerRow + 1;
        foreach (var r in data.Rows)
        {
            foreach (var line in r.Lines)
            {
                sheet.Cell(row, 1).Value = r.EmployeeName;
                sheet.Cell(row, 2).SetValue(r.DocumentNumber);
                sheet.Cell(row, 3).Value = line.ConceptLabel;
                sheet.Cell(row, 4).Value = line.Description;
                Money(sheet.Cell(row, 5), line.Amount);
                if (line.Amount < 0) sheet.Cell(row, 5).Style.Font.SetFontColor(XLColor.FromHtml("#C00000"));
                row++;
            }

            sheet.Cell(row, 4).Value = "Neto a pagar";
            Money(sheet.Cell(row, 5), r.NetAmount);
            var netRange = sheet.Range(row, 1, row, 5);
            netRange.Style.Font.SetBold();
            netRange.Style.Fill.SetBackgroundColor(NetColor);
            row += 2;
        }

        sheet.Columns(1, 5).AdjustToContents(headerRow, Math.Max(headerRow, row));
        sheet.SheetView.FreezeRows(headerRow);
        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0);
    }

    private static void Group(IXLWorksheet sheet, int row, int fromCol, int toCol, string title, XLColor color)
    {
        var range = sheet.Range(row, fromCol, row, toCol);
        if (fromCol != toCol) range.Merge();
        range.FirstCell().Value = title;
        range.Style.Font.SetBold();
        range.Style.Fill.SetBackgroundColor(color);
        range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    }

    private static IXLCell Money(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.NumberFormat.Format = MoneyFormat;
        return cell;
    }
}

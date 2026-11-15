public record PayrollExportRow
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;

    /// <summary>Sueldo mensual del trabajador (referencial).</summary>
    public decimal MonthlySalary { get; set; }

    // Ingresos
    public decimal BaseAmount { get; set; }
    public decimal ExtraHours { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossAmount { get; set; }

    // Descuentos (en positivo)
    public decimal AbsenceDiscount { get; set; }
    public decimal LateArrivalDiscount { get; set; }
    public decimal LoanDiscount { get; set; }
    public decimal AdvanceDiscount { get; set; }
    public decimal OtherDiscount { get; set; }
    public decimal TotalDiscounts { get; set; }

    public decimal NetAmount { get; set; }

    public List<PayrollExportDetailLine> Lines { get; set; } = new();
}

public record PayrollExportDetailLine
{
    public PayrollConceptType ConceptType { get; set; }
    public string ConceptLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public record PayrollExportData
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyDocument { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string FrequencyLabel { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsClosed { get; set; }
    public List<PayrollExportRow> Rows { get; set; } = new();

    public decimal Sum(Func<PayrollExportRow, decimal> selector) => Rows.Sum(selector);
}

public enum PayrollExportStatus
{
    Ok,
    PeriodNotFound,
    NoPayrolls
}

public record PayrollExportResult(PayrollExportStatus Status, byte[]? Content = null, string? FileName = null);

public static class PayrollExportLabels
{
    public static string Frequency(PayrollFrequency frequency) => frequency switch
    {
        PayrollFrequency.Biweekly => "Quincenal",
        PayrollFrequency.Monthly => "Mensual",
        _ => frequency.ToString()
    };

    public static string Concept(PayrollConceptType type) => type switch
    {
        PayrollConceptType.BaseSalary => "Sueldo Base",
        PayrollConceptType.ExtraHours => "Horas Extras",
        PayrollConceptType.Bonus => "Bono",
        PayrollConceptType.LoanDiscount => "Descuento por Préstamo",
        PayrollConceptType.AdvanceDiscount => "Descuento por Adelanto",
        PayrollConceptType.AbsenceDiscount => "Descuento por Falta",
        PayrollConceptType.LateArrivalDiscount => "Descuento por Tardanza",
        PayrollConceptType.OtherDiscount => "Otros Descuentos",
        _ => type.ToString()
    };
}

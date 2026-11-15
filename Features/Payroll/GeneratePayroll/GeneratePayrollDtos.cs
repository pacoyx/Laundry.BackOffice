public record PayrollDetailLineDto
{
    public PayrollConceptType ConceptType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public record PayrollSummaryDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal Discounts { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<PayrollDetailLineDto> Details { get; set; } = new();
}

public record GeneratePayrollResponseDto
{
    public long PayrollPeriodId { get; set; }
    public int GeneratedCount { get; set; }
    public int SkippedCount { get; set; }

    /// <summary>Trabajadores ad honorem omitidos (no se les genera planilla).</summary>
    public int AdHonoremSkippedCount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public List<PayrollSummaryDto> Payrolls { get; set; } = new();
}

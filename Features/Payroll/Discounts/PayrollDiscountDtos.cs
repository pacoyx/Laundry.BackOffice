public record CreatePayrollDiscountRequestDto
{
    public long EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public record PayrollDiscountDto
{
    public long Id { get; set; }
    public long PayrollPeriodId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
}

public record PayrollDiscountChangeResponseDto
{
    public PayrollDiscountDto? Discount { get; set; }

    /// <summary>
    /// Planilla del trabajador recalculada. Es null si aún no se generó la planilla del trabajador en el periodo:
    /// en ese caso el descuento se aplicará al generarla.
    /// </summary>
    public PayrollSummaryDto? Payroll { get; set; }
}

public enum PayrollDiscountStatus
{
    Ok,
    PeriodNotFound,
    PeriodClosed,
    EmployeeNotFound,
    DiscountNotFound,
    NegativeNet,
    EmployeeAdHonorem
}

public record PayrollDiscountResult(PayrollDiscountStatus Status, PayrollDiscountChangeResponseDto? Data = null, string? Message = null);

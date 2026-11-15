public enum DeletePayrollStatus
{
    Deleted,
    PeriodNotFound,
    PeriodClosed,
    NothingToDelete
}

public record DeletePayrollResponseDto
{
    public long PayrollPeriodId { get; set; }
    public int DeletedPayrolls { get; set; }
    public int RevertedInstallments { get; set; }
    public int ReopenedLoans { get; set; }
}

public record DeletePayrollResult(DeletePayrollStatus Status, DeletePayrollResponseDto? Data = null);

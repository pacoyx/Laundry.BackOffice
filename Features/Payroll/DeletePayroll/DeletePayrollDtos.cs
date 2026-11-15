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

    /// <summary>Descuentos eliminados (solo al eliminar el periodo completo).</summary>
    public int DeletedDiscounts { get; set; }

    /// <summary>True cuando también se eliminó el periodo.</summary>
    public bool PeriodDeleted { get; set; }
}

public record DeletePayrollResult(DeletePayrollStatus Status, DeletePayrollResponseDto? Data = null);

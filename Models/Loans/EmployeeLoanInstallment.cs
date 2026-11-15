public class EmployeeLoanInstallment
{
    public long Id { get; set; }

    public long EmployeeLoanId { get; set; }

    public int InstallmentNumber { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    /// <summary>
    /// Planilla en la que se descontó la cuota. Si la planilla se elimina, la cuota vuelve a quedar pendiente.
    /// </summary>
    public long? PayrollId { get; set; }

    public DateTime? PaidAt { get; set; }

    public EmployeeLoan? EmployeeLoan { get; set; }

    public Payroll? Payroll { get; set; }
}

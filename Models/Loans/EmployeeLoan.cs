using System.ComponentModel.DataAnnotations;

public class EmployeeLoan
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public LoanType LoanType { get; set; }

    public DateTime LoanDate { get; set; }

    public decimal Amount { get; set; }

    public decimal Balance { get; set; }

    [MaxLength(500)]
    public string? Observation { get; set; }

    public int InstallmentCount { get; set; } = 1;

    public bool IsClosed { get; set; }

    public Employee? Employee { get; set; }

    public List<EmployeeLoanInstallment> Installments { get; set; } = new();
}

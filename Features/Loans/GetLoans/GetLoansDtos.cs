public record LoanListItemDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LoanType LoanType { get; set; }
    public DateTime LoanDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string? Observation { get; set; }
    public int InstallmentCount { get; set; }
    public int PaidInstallments { get; set; }
    public bool IsClosed { get; set; }
    public List<LoanInstallmentDto> Installments { get; set; } = new();
}

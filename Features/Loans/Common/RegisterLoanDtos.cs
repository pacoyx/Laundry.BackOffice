public record RegisterLoanRequestDto
{
    public DateTime LoanDate { get; set; }
    public decimal Amount { get; set; }
    public string? Observation { get; set; }

    /// <summary>
    /// Número de cuotas en que se descontará el préstamo. Se descuenta una cuota en cada planilla
    /// generada a partir de la fecha del préstamo (quincenal o mensual). Los adelantos siempre son 1 cuota.
    /// </summary>
    public int InstallmentCount { get; set; } = 1;
}

public record LoanInstallmentDto
{
    public long Id { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public long? PayrollId { get; set; }
    public long? PayrollPeriodId { get; set; }
    public string? PayrollPeriodName { get; set; }
    public DateTime? PaidAt { get; set; }
}

public record EmployeeLoanResponseDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
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

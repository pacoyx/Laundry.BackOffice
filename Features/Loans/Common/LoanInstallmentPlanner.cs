public static class LoanInstallmentPlanner
{
    /// <summary>
    /// Divide el monto en cuotas iguales redondeadas a 2 decimales; la última cuota absorbe la diferencia
    /// de redondeo para que la suma sea exactamente el monto del préstamo.
    /// </summary>
    public static List<EmployeeLoanInstallment> Build(decimal amount, int installmentCount)
    {
        if (installmentCount < 1) installmentCount = 1;

        var regular = decimal.Round(amount / installmentCount, 2, MidpointRounding.AwayFromZero);
        var installments = new List<EmployeeLoanInstallment>(installmentCount);

        for (var i = 1; i <= installmentCount; i++)
        {
            var value = i < installmentCount ? regular : amount - regular * (installmentCount - 1);
            installments.Add(new EmployeeLoanInstallment
            {
                InstallmentNumber = i,
                Amount = value,
                IsPaid = false
            });
        }

        return installments;
    }

    public static EmployeeLoanResponseDto ToResponse(EmployeeLoan loan) => new()
    {
        Id = loan.Id,
        EmployeeId = loan.EmployeeId,
        LoanType = loan.LoanType,
        LoanDate = loan.LoanDate,
        Amount = loan.Amount,
        Balance = loan.Balance,
        Observation = loan.Observation,
        InstallmentCount = loan.InstallmentCount,
        PaidInstallments = loan.Installments.Count(i => i.IsPaid),
        IsClosed = loan.IsClosed,
        Installments = loan.Installments
            .OrderBy(i => i.InstallmentNumber)
            .Select(i => new LoanInstallmentDto
            {
                Id = i.Id,
                InstallmentNumber = i.InstallmentNumber,
                Amount = i.Amount,
                IsPaid = i.IsPaid,
                PayrollId = i.PayrollId,
                PayrollPeriodId = i.Payroll?.PayrollPeriodId,
                PayrollPeriodName = i.Payroll?.PayrollPeriod?.Name,
                PaidAt = i.PaidAt
            })
            .ToList()
    };
}

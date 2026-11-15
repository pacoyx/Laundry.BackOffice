using Microsoft.EntityFrameworkCore;

public class RegisterLoanService : IRegisterLoanService
{
    private readonly RecepcionDbContext _context;

    public RegisterLoanService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterLoanResult> RegisterLoanAsync(long employeeId, LoanType loanType, RegisterLoanRequestDto request)
    {
        var contractType = await _context.Employees
            .Where(x => x.Id == employeeId)
            .Select(x => (EmployeeContractType?)x.ContractType)
            .FirstOrDefaultAsync();
        if (contractType == null)
        {
            return new RegisterLoanResult(RegisterLoanStatus.EmployeeNotFound);
        }

        // Los trabajadores ad honorem no tienen planilla de la cual descontar.
        if (contractType == EmployeeContractType.AdHonorem)
        {
            return new RegisterLoanResult(RegisterLoanStatus.EmployeeAdHonorem);
        }

        // Los adelantos de sueldo se descuentan completos en la siguiente planilla.
        var installmentCount = loanType == LoanType.SalaryAdvance ? 1 : Math.Max(1, request.InstallmentCount);

        var loan = new EmployeeLoan
        {
            EmployeeId = employeeId,
            LoanType = loanType,
            LoanDate = request.LoanDate,
            Amount = request.Amount,
            Balance = request.Amount,
            Observation = request.Observation,
            InstallmentCount = installmentCount,
            IsClosed = false,
            Installments = LoanInstallmentPlanner.Build(request.Amount, installmentCount)
        };

        _context.EmployeeLoans.Add(loan);
        await _context.SaveChangesAsync();

        return new RegisterLoanResult(RegisterLoanStatus.Ok, LoanInstallmentPlanner.ToResponse(loan));
    }
}

using Microsoft.EntityFrameworkCore;

public class RegisterLoanService : IRegisterLoanService
{
    private readonly RecepcionDbContext _context;

    public RegisterLoanService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeLoanResponseDto?> RegisterLoanAsync(long employeeId, LoanType loanType, RegisterLoanRequestDto request)
    {
        var employeeExists = await _context.Employees.AnyAsync(x => x.Id == employeeId);
        if (!employeeExists)
        {
            return null;
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

        return LoanInstallmentPlanner.ToResponse(loan);
    }
}

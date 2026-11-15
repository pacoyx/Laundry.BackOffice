using Microsoft.EntityFrameworkCore;

public class GetLoansService : IGetLoansService
{
    private readonly RecepcionDbContext _context;

    public GetLoansService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LoanListItemDto>> GetLoansAsync(long? employeeId, LoanType? loanType, bool? isClosed)
    {
        var query = _context.EmployeeLoans
            .AsNoTracking()
            .Include(x => x.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (loanType.HasValue)
        {
            query = query.Where(x => x.LoanType == loanType.Value);
        }

        if (isClosed.HasValue)
        {
            query = query.Where(x => x.IsClosed == isClosed.Value);
        }

        return await query
            .OrderByDescending(x => x.LoanDate)
            .Select(x => new LoanListItemDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee!.FirstName + " " + x.Employee.LastName,
                LoanType = x.LoanType,
                LoanDate = x.LoanDate,
                Amount = x.Amount,
                Balance = x.Balance,
                Observation = x.Observation,
                InstallmentCount = x.InstallmentCount,
                PaidInstallments = x.Installments.Count(i => i.IsPaid),
                IsClosed = x.IsClosed,
                Installments = x.Installments
                    .OrderBy(i => i.InstallmentNumber)
                    .Select(i => new LoanInstallmentDto
                    {
                        Id = i.Id,
                        InstallmentNumber = i.InstallmentNumber,
                        Amount = i.Amount,
                        IsPaid = i.IsPaid,
                        PayrollId = i.PayrollId,
                        PayrollPeriodId = i.Payroll != null ? (long?)i.Payroll.PayrollPeriodId : null,
                        PayrollPeriodName = i.Payroll != null ? i.Payroll.PayrollPeriod!.Name : null,
                        PaidAt = i.PaidAt
                    })
                    .ToList()
            })
            .ToListAsync();
    }
}

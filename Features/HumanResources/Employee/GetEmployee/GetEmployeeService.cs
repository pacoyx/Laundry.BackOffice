using Microsoft.EntityFrameworkCore;

public class GetEmployeeService : IGetEmployeeService
{
    private readonly RecepcionDbContext _context;

    public GetEmployeeService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDetailResponseDto?> GetEmployeeAsync(long id)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Area)
            .Include(x => x.Position)
            .Include(x => x.EmergencyContacts)
            .Include(x => x.BankAccounts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (employee == null)
        {
            return null;
        }

        return new EmployeeDetailResponseDto
        {
            Id = employee.Id,
            Code = employee.Code,
            DocumentNumber = employee.DocumentNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            PhoneNumber = employee.PhoneNumber,
            Address = employee.Address,
            BirthDate = employee.BirthDate,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            IsActive = employee.IsActive,
            AreaId = employee.AreaId,
            AreaName = employee.Area.Name,
            PositionId = employee.PositionId,
            PositionName = employee.Position.Name,
            ContractType = employee.ContractType,
            BaseSalary = employee.BaseSalary,
            EmergencyContacts = employee.EmergencyContacts?.Select(c => new EmergencyContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Relationship = c.Relationship,
                PhoneNumber = c.PhoneNumber
            }).ToList() ?? new(),
            BankAccounts = employee.BankAccounts?.Select(a => new BankAccountDto
            {
                Id = a.Id,
                BankName = a.BankName,
                AccountNumber = a.AccountNumber,
                Cci = a.Cci
            }).ToList() ?? new()
        };
    }
}

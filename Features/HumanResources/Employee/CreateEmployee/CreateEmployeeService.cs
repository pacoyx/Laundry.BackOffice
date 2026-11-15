using Microsoft.EntityFrameworkCore;

public class CreateEmployeeService : ICreateEmployeeService
{
    private readonly RecepcionDbContext _context;

    public CreateEmployeeService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeRequestDto request)
    {
        var employee = new Employee
        {
            Code = request.Code,
            DocumentNumber = request.DocumentNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            BirthDate = request.BirthDate,
            HireDate = request.HireDate,
            IsActive = true,
            AreaId = request.AreaId,
            PositionId = request.PositionId,
            ContractType = request.ContractType,
            // Ad honorem: no recibe pago, el sueldo base siempre es 0.
            BaseSalary = request.ContractType == EmployeeContractType.AdHonorem ? 0m : request.BaseSalary
        };

        if (request.EmergencyContacts != null && request.EmergencyContacts.Count > 0)
        {
            employee.EmergencyContacts = request.EmergencyContacts.Select(c => new EmployeeEmergencyContact
            {
                Name = c.Name,
                Relationship = c.Relationship,
                PhoneNumber = c.PhoneNumber
            }).ToList();
        }

        if (request.BankAccounts != null && request.BankAccounts.Count > 0)
        {
            employee.BankAccounts = request.BankAccounts.Select(a => new EmployeeBankAccount
            {
                BankName = a.BankName,
                AccountNumber = a.AccountNumber,
                Cci = a.Cci
            }).ToList();
        }

        _context.Employees.Add(employee);

        _context.EmployeeSalaryHistories.Add(new EmployeeSalaryHistory
        {
            Employee = employee,
            Salary = employee.BaseSalary,
            EffectiveDate = employee.HireDate
        });

        await _context.SaveChangesAsync();

        var area = await _context.Areas.AsNoTracking().FirstAsync(x => x.Id == employee.AreaId);
        var position = await _context.Positions.AsNoTracking().FirstAsync(x => x.Id == employee.PositionId);

        return new EmployeeResponseDto
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
            AreaName = area.Name,
            PositionId = employee.PositionId,
            PositionName = position.Name,
            ContractType = employee.ContractType,
            BaseSalary = employee.BaseSalary
        };
    }
}

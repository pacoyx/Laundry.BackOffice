using Microsoft.EntityFrameworkCore;

public class PayrollDiscountService : IPayrollDiscountService
{
    private readonly RecepcionDbContext _context;

    public PayrollDiscountService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PayrollDiscountDto>?> ListAsync(long payrollPeriodId, long? employeeId)
    {
        var periodExists = await _context.PayrollPeriods.AnyAsync(x => x.Id == payrollPeriodId);
        if (!periodExists)
        {
            return null;
        }

        var query = _context.PayrollDiscounts.AsNoTracking().Where(x => x.PayrollPeriodId == payrollPeriodId);
        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        return await query
            .OrderBy(x => x.Employee!.LastName).ThenBy(x => x.Employee!.FirstName).ThenBy(x => x.Id)
            .Select(x => new PayrollDiscountDto
            {
                Id = x.Id,
                PayrollPeriodId = x.PayrollPeriodId,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee!.FirstName + " " + x.Employee.LastName,
                Description = x.Description,
                Amount = x.Amount,
                Created = x.Created,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync();
    }

    public async Task<PayrollDiscountResult> AddAsync(long payrollPeriodId, CreatePayrollDiscountRequestDto request)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null) return new PayrollDiscountResult(PayrollDiscountStatus.PeriodNotFound);
        if (period.IsClosed) return new PayrollDiscountResult(PayrollDiscountStatus.PeriodClosed);

        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == request.EmployeeId);
        if (employee == null) return new PayrollDiscountResult(PayrollDiscountStatus.EmployeeNotFound);
        if (employee.ContractType == EmployeeContractType.AdHonorem) return new PayrollDiscountResult(PayrollDiscountStatus.EmployeeAdHonorem);

        var discount = new PayrollDiscount
        {
            PayrollPeriodId = payrollPeriodId,
            EmployeeId = request.EmployeeId,
            Description = request.Description.Trim(),
            Amount = request.Amount
        };

        var payroll = await LoadPayrollAsync(payrollPeriodId, request.EmployeeId);
        if (payroll != null)
        {
            var nuevoNeto = payroll.NetAmount - discount.Amount;
            if (nuevoNeto < 0)
            {
                return new PayrollDiscountResult(PayrollDiscountStatus.NegativeNet,
                    Message: $"El descuento deja el neto a pagar en negativo ({nuevoNeto:N2}). El neto actual es {payroll.NetAmount:N2}.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.PayrollDiscounts.Add(discount);
        await _context.SaveChangesAsync();

        if (payroll != null)
        {
            await RebuildDiscountLinesAsync(payroll);
        }

        await transaction.CommitAsync();

        return new PayrollDiscountResult(PayrollDiscountStatus.Ok, new PayrollDiscountChangeResponseDto
        {
            Discount = new PayrollDiscountDto
            {
                Id = discount.Id,
                PayrollPeriodId = discount.PayrollPeriodId,
                EmployeeId = discount.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Description = discount.Description,
                Amount = discount.Amount,
                Created = discount.Created,
                CreatedBy = discount.CreatedBy
            },
            Payroll = payroll == null ? null : PayrollDiscountCalculator.ToSummary(payroll, employee)
        });
    }

    public async Task<PayrollDiscountResult> DeleteAsync(long payrollPeriodId, long discountId)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null) return new PayrollDiscountResult(PayrollDiscountStatus.PeriodNotFound);
        if (period.IsClosed) return new PayrollDiscountResult(PayrollDiscountStatus.PeriodClosed);

        var discount = await _context.PayrollDiscounts
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == discountId && x.PayrollPeriodId == payrollPeriodId);
        if (discount == null) return new PayrollDiscountResult(PayrollDiscountStatus.DiscountNotFound);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.PayrollDiscounts.Remove(discount);
        await _context.SaveChangesAsync();

        var payroll = await LoadPayrollAsync(payrollPeriodId, discount.EmployeeId);
        if (payroll != null)
        {
            await RebuildDiscountLinesAsync(payroll);
        }

        await transaction.CommitAsync();

        return new PayrollDiscountResult(PayrollDiscountStatus.Ok, new PayrollDiscountChangeResponseDto
        {
            Payroll = payroll == null ? null : PayrollDiscountCalculator.ToSummary(payroll, discount.Employee!)
        });
    }

    private Task<Payroll?> LoadPayrollAsync(long payrollPeriodId, long employeeId) =>
        _context.Payrolls
            .Include(x => x.PayrollDetails)
            .FirstOrDefaultAsync(x => x.PayrollPeriodId == payrollPeriodId && x.EmployeeId == employeeId);

    /// <summary>
    /// Reemplaza las líneas "Otros descuentos" de la planilla por las registradas en la tabla y recalcula totales.
    /// Solo cambian descuentos y neto; el resto de conceptos (sueldo, horas extra, cuotas de préstamos) no se toca.
    /// </summary>
    private async Task RebuildDiscountLinesAsync(Payroll payroll)
    {
        var discounts = await _context.PayrollDiscounts
            .AsNoTracking()
            .Where(x => x.PayrollPeriodId == payroll.PayrollPeriodId && x.EmployeeId == payroll.EmployeeId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        payroll.PayrollDetails ??= new List<PayrollDetail>();
        var anteriores = payroll.PayrollDetails.Where(d => d.ConceptType == PayrollConceptType.OtherDiscount).ToList();
        foreach (var linea in anteriores)
        {
            payroll.PayrollDetails.Remove(linea);
            _context.PayrollDetails.Remove(linea);
        }

        payroll.PayrollDetails.AddRange(discounts.Select(PayrollDiscountCalculator.ToDetail));
        PayrollDiscountCalculator.RecalculateTotals(payroll);

        await _context.SaveChangesAsync();
    }
}

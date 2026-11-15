using Microsoft.EntityFrameworkCore;

public class GeneratePayrollService : IGeneratePayrollService
{
    private const int StandardDaysPerMonth = 30;
    private const int StandardHoursPerDay = 8;

    private readonly RecepcionDbContext _context;

    public GeneratePayrollService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<GeneratePayrollResponseDto?> GeneratePayrollAsync(long payrollPeriodId)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null)
        {
            return null;
        }

        var alreadyGeneratedEmployeeIds = await _context.Payrolls
            .Where(x => x.PayrollPeriodId == payrollPeriodId)
            .Select(x => x.EmployeeId)
            .ToListAsync();

        var eligibleEmployees = await _context.Employees
            .Where(x => x.IsActive
                && x.HireDate <= period.EndDate.ToDateTime(TimeOnly.MinValue)
                && (x.TerminationDate == null || x.TerminationDate >= period.StartDate.ToDateTime(TimeOnly.MinValue))
                && !alreadyGeneratedEmployeeIds.Contains(x.Id))
            .ToListAsync();

        // Los trabajadores ad honorem no reciben pago: no se les genera planilla.
        var adHonoremCount = eligibleEmployees.Count(x => x.ContractType == EmployeeContractType.AdHonorem);
        var employees = eligibleEmployees.Where(x => x.ContractType != EmployeeContractType.AdHonorem).ToList();

        var periodDays = period.EndDate.DayNumber - period.StartDate.DayNumber + 1;
        var summaries = new List<PayrollSummaryDto>();
        var generatedPayrolls = new List<Payroll>();

        foreach (var employee in employees)
        {
            var dailyRate = employee.BaseSalary / StandardDaysPerMonth;
            var hourlyRate = dailyRate / StandardHoursPerDay;

            var baseAmount = decimal.Round(dailyRate * periodDays, 2);

            var extraHours = await _context.EmployeeExtraHours
                .Where(x => x.EmployeeId == employee.Id
                    && x.Approved
                    && x.Date >= period.StartDate && x.Date <= period.EndDate)
                .SumAsync(x => (decimal?)x.Hours) ?? 0;
            var extraHoursAmount = decimal.Round(extraHours * hourlyRate, 2);

            var absenceHours = await _context.AttendanceIncidents
                .Where(x => x.EmployeeId == employee.Id
                    && x.Type == AttendanceIncidentType.Absence
                    && x.Date >= period.StartDate && x.Date <= period.EndDate)
                .SumAsync(x => (decimal?)x.Hours) ?? 0;
            var absenceDiscount = decimal.Round(absenceHours * hourlyRate, 2);

            var lateArrivalHours = await _context.AttendanceIncidents
                .Where(x => x.EmployeeId == employee.Id
                    && x.Type == AttendanceIncidentType.LateArrival
                    && x.Date >= period.StartDate && x.Date <= period.EndDate)
                .SumAsync(x => (decimal?)x.Hours) ?? 0;
            var lateArrivalDiscount = decimal.Round(lateArrivalHours * hourlyRate, 2);

            var payroll = new Payroll
            {
                PayrollPeriodId = period.Id,
                EmployeeId = employee.Id,
                GeneratedAt = DateTime.UtcNow
            };

            // Préstamos/adelantos vigentes otorgados hasta el fin del periodo: se descuenta UNA cuota
            // pendiente por préstamo en cada planilla (quincenal o mensual).
            var periodEnd = period.EndDate.ToDateTime(TimeOnly.MaxValue);
            var openLoans = await _context.EmployeeLoans
                .Include(x => x.Installments)
                .Where(x => x.EmployeeId == employee.Id && !x.IsClosed && x.Balance > 0 && x.LoanDate <= periodEnd)
                .OrderBy(x => x.LoanDate)
                .ToListAsync();

            var loanLines = new List<PayrollDetail>();
            var loanDiscount = 0m;
            var advanceDiscount = 0m;

            foreach (var loan in openLoans)
            {
                var installment = loan.Installments
                    .Where(i => !i.IsPaid)
                    .OrderBy(i => i.InstallmentNumber)
                    .FirstOrDefault();

                if (installment == null)
                {
                    // Saldo sin cuota pendiente (datos anteriores al manejo de cuotas): se agenda el saldo como cuota.
                    installment = new EmployeeLoanInstallment
                    {
                        InstallmentNumber = loan.Installments.Count == 0 ? 1 : loan.Installments.Max(i => i.InstallmentNumber) + 1,
                        Amount = loan.Balance
                    };
                    loan.Installments.Add(installment);
                    loan.InstallmentCount = loan.Installments.Count;
                }

                var amount = Math.Min(installment.Amount, loan.Balance);

                installment.IsPaid = true;
                installment.PaidAt = DateTime.UtcNow;
                installment.Payroll = payroll;

                loan.Balance -= amount;
                if (loan.Balance <= 0)
                {
                    loan.Balance = 0;
                    loan.IsClosed = true;
                }

                var isAdvance = loan.LoanType == LoanType.SalaryAdvance;
                if (isAdvance) advanceDiscount += amount; else loanDiscount += amount;

                var label = isAdvance ? "Adelanto" : "Préstamo";
                loanLines.Add(new PayrollDetail
                {
                    ConceptType = isAdvance ? PayrollConceptType.AdvanceDiscount : PayrollConceptType.LoanDiscount,
                    Description = $"{label} del {loan.LoanDate:dd/MM/yyyy} - cuota {installment.InstallmentNumber}/{loan.InstallmentCount}",
                    Amount = -amount
                });
            }

            // Otros descuentos registrados para el trabajador en este periodo (antes de generar o de una
            // generación anterior que se eliminó): se aplican como líneas propias.
            var otherDiscountLines = (await _context.PayrollDiscounts
                    .AsNoTracking()
                    .Where(x => x.PayrollPeriodId == period.Id && x.EmployeeId == employee.Id)
                    .OrderBy(x => x.Id)
                    .ToListAsync())
                .Select(PayrollDiscountCalculator.ToDetail)
                .ToList();
            var otherDiscounts = -otherDiscountLines.Sum(x => x.Amount);

            var grossAmount = baseAmount + extraHoursAmount;
            var totalDiscounts = absenceDiscount + lateArrivalDiscount + loanDiscount + advanceDiscount + otherDiscounts;
            var netAmount = grossAmount - totalDiscounts;

            payroll.GrossAmount = grossAmount;
            payroll.Discounts = totalDiscounts;
            payroll.NetAmount = netAmount;

            var details = new List<PayrollDetail>
            {
                new() { ConceptType = PayrollConceptType.BaseSalary, Description = "Sueldo base", Amount = baseAmount }
            };

            if (extraHoursAmount > 0)
            {
                details.Add(new PayrollDetail { ConceptType = PayrollConceptType.ExtraHours, Description = $"Horas extras ({extraHours}h)", Amount = extraHoursAmount });
            }

            if (absenceDiscount > 0)
            {
                details.Add(new PayrollDetail { ConceptType = PayrollConceptType.AbsenceDiscount, Description = $"Descuento por faltas ({absenceHours}h)", Amount = -absenceDiscount });
            }

            if (lateArrivalDiscount > 0)
            {
                details.Add(new PayrollDetail { ConceptType = PayrollConceptType.LateArrivalDiscount, Description = $"Descuento por tardanzas ({lateArrivalHours}h)", Amount = -lateArrivalDiscount });
            }

            details.AddRange(loanLines);
            details.AddRange(otherDiscountLines);

            payroll.PayrollDetails = details;

            _context.Payrolls.Add(payroll);
            generatedPayrolls.Add(payroll);

            summaries.Add(new PayrollSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                GrossAmount = grossAmount,
                Discounts = totalDiscounts,
                NetAmount = netAmount,
                GeneratedAt = payroll.GeneratedAt,
                Details = details.Select(d => new PayrollDetailLineDto
                {
                    ConceptType = d.ConceptType,
                    Description = d.Description,
                    Amount = d.Amount
                }).ToList()
            });
        }

        await _context.SaveChangesAsync();

        for (var i = 0; i < summaries.Count; i++)
        {
            summaries[i] = summaries[i] with { Id = generatedPayrolls[i].Id };
        }

        return new GeneratePayrollResponseDto
        {
            PayrollPeriodId = period.Id,
            GeneratedCount = summaries.Count,
            SkippedCount = alreadyGeneratedEmployeeIds.Count,
            AdHonoremSkippedCount = adHonoremCount,
            TotalNetAmount = summaries.Sum(x => x.NetAmount),
            Payrolls = summaries
        };
    }
}

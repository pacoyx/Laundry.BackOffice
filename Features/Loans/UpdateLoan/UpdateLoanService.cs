using Microsoft.EntityFrameworkCore;

public class UpdateLoanService : IUpdateLoanService
{
    private readonly RecepcionDbContext _context;

    public UpdateLoanService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateLoanResult> UpdateLoanAsync(long loanId, RegisterLoanRequestDto request)
    {
        var loan = await _context.EmployeeLoans
            .Include(x => x.Installments)
            .FirstOrDefaultAsync(x => x.Id == loanId);
        if (loan == null)
        {
            return new UpdateLoanResult(UpdateLoanStatus.LoanNotFound);
        }

        // Solo se puede editar mientras ninguna cuota haya sido descontada en planilla.
        if (LoanModificationGuard.HasCollectedInstallments(loan))
        {
            return new UpdateLoanResult(UpdateLoanStatus.HasCollectedInstallments);
        }

        // Los trabajadores ad honorem no tienen planilla de la cual descontar.
        var contractType = await _context.Employees
            .Where(x => x.Id == loan.EmployeeId)
            .Select(x => (EmployeeContractType?)x.ContractType)
            .FirstOrDefaultAsync();
        if (contractType == EmployeeContractType.AdHonorem)
        {
            return new UpdateLoanResult(UpdateLoanStatus.EmployeeAdHonorem);
        }

        // Los adelantos de sueldo se descuentan completos en la siguiente planilla.
        var installmentCount = loan.LoanType == LoanType.SalaryAdvance ? 1 : Math.Max(1, request.InstallmentCount);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Se borran en BD solo las cuotas pendientes. Si una planilla cobró alguna cuota después de la
        // verificación anterior, el conteo no coincide y se cancela la edición.
        if (!await LoanModificationGuard.TryDeletePendingInstallmentsAsync(_context, loanId))
        {
            await transaction.RollbackAsync();
            return new UpdateLoanResult(UpdateLoanStatus.HasCollectedInstallments);
        }

        // Las cuotas cargadas ya no existen en BD: se dejan de rastrear para que EF no intente borrarlas de nuevo.
        foreach (var installment in loan.Installments)
        {
            _context.Entry(installment).State = EntityState.Detached;
        }

        loan.LoanDate = request.LoanDate;
        loan.Amount = request.Amount;
        loan.Balance = request.Amount;
        loan.Observation = request.Observation;
        loan.InstallmentCount = installmentCount;
        loan.IsClosed = false;
        loan.Installments = LoanInstallmentPlanner.Build(request.Amount, installmentCount);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return new UpdateLoanResult(UpdateLoanStatus.Ok, LoanInstallmentPlanner.ToResponse(loan));
    }
}

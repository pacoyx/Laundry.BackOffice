using Microsoft.EntityFrameworkCore;

public class DeletePayrollService : IDeletePayrollService
{
    private readonly RecepcionDbContext _context;

    public DeletePayrollService(RecepcionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Elimina todas las planillas generadas del periodo. Las cuotas de préstamos/adelantos descontadas en ellas
    /// vuelven a quedar pendientes, se restituye el saldo del préstamo y se reabre si estaba cancelado, para que
    /// la cuota se descuente en la próxima planilla que se genere.
    /// </summary>
    public async Task<DeletePayrollResult> DeletePayrollAsync(long payrollPeriodId)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null)
        {
            return new DeletePayrollResult(DeletePayrollStatus.PeriodNotFound);
        }

        if (period.IsClosed)
        {
            return new DeletePayrollResult(DeletePayrollStatus.PeriodClosed);
        }

        var payrolls = await _context.Payrolls
            .Where(x => x.PayrollPeriodId == payrollPeriodId)
            .ToListAsync();

        if (payrolls.Count == 0)
        {
            return new DeletePayrollResult(DeletePayrollStatus.NothingToDelete);
        }

        var payrollIds = payrolls.Select(x => x.Id).ToList();

        var installments = await _context.EmployeeLoanInstallments
            .Include(x => x.EmployeeLoan)
            .Where(x => x.PayrollId != null && payrollIds.Contains(x.PayrollId.Value))
            .ToListAsync();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var reopenedLoans = new HashSet<long>();
        foreach (var installment in installments)
        {
            var loan = installment.EmployeeLoan!;

            loan.Balance = Math.Min(loan.Amount, loan.Balance + installment.Amount);
            if (loan.IsClosed && loan.Balance > 0)
            {
                loan.IsClosed = false;
                reopenedLoans.Add(loan.Id);
            }

            installment.IsPaid = false;
            installment.PaidAt = null;
            installment.PayrollId = null;
        }

        // Primero se liberan las cuotas (FK Restrict) y luego se eliminan las planillas (detalles en cascada).
        await _context.SaveChangesAsync();

        _context.Payrolls.RemoveRange(payrolls);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return new DeletePayrollResult(DeletePayrollStatus.Deleted, new DeletePayrollResponseDto
        {
            PayrollPeriodId = payrollPeriodId,
            DeletedPayrolls = payrolls.Count,
            RevertedInstallments = installments.Count,
            ReopenedLoans = reopenedLoans.Count
        });
    }
}

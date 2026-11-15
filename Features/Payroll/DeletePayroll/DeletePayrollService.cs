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
    /// la cuota se descuente en la próxima planilla que se genere. El periodo y sus descuentos se conservan.
    /// </summary>
    public async Task<DeletePayrollResult> DeletePayrollAsync(long payrollPeriodId)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null) return new DeletePayrollResult(DeletePayrollStatus.PeriodNotFound);
        if (period.IsClosed) return new DeletePayrollResult(DeletePayrollStatus.PeriodClosed);

        var hasPayrolls = await _context.Payrolls.AnyAsync(x => x.PayrollPeriodId == payrollPeriodId);
        if (!hasPayrolls) return new DeletePayrollResult(DeletePayrollStatus.NothingToDelete);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var response = await RevertAndDeletePayrollsAsync(payrollPeriodId);
        await transaction.CommitAsync();

        return new DeletePayrollResult(DeletePayrollStatus.Deleted, response);
    }

    /// <summary>
    /// Elimina el periodo completo: revierte cuotas y elimina las planillas generadas (si las hay), elimina los
    /// descuentos registrados del periodo y finalmente el periodo. Todo en una sola transacción.
    /// </summary>
    public async Task<DeletePayrollResult> DeletePeriodAsync(long payrollPeriodId)
    {
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == payrollPeriodId);
        if (period == null) return new DeletePayrollResult(DeletePayrollStatus.PeriodNotFound);
        if (period.IsClosed) return new DeletePayrollResult(DeletePayrollStatus.PeriodClosed);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var response = await RevertAndDeletePayrollsAsync(payrollPeriodId);

        var discounts = await _context.PayrollDiscounts.Where(x => x.PayrollPeriodId == payrollPeriodId).ToListAsync();
        _context.PayrollDiscounts.RemoveRange(discounts);
        _context.PayrollPeriods.Remove(period);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        response.DeletedDiscounts = discounts.Count;
        response.PeriodDeleted = true;
        return new DeletePayrollResult(DeletePayrollStatus.Deleted, response);
    }

    /// <summary>
    /// Revierte las cuotas descontadas en las planillas del periodo y elimina esas planillas (detalles en cascada).
    /// Debe ejecutarse dentro de una transacción abierta por el llamador.
    /// </summary>
    private async Task<DeletePayrollResponseDto> RevertAndDeletePayrollsAsync(long payrollPeriodId)
    {
        var payrolls = await _context.Payrolls
            .Where(x => x.PayrollPeriodId == payrollPeriodId)
            .ToListAsync();

        var payrollIds = payrolls.Select(x => x.Id).ToList();

        var installments = payrollIds.Count == 0
            ? new List<EmployeeLoanInstallment>()
            : await _context.EmployeeLoanInstallments
                .Include(x => x.EmployeeLoan)
                .Where(x => x.PayrollId != null && payrollIds.Contains(x.PayrollId.Value))
                .ToListAsync();

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

        if (payrolls.Count > 0)
        {
            _context.Payrolls.RemoveRange(payrolls);
            await _context.SaveChangesAsync();
        }

        return new DeletePayrollResponseDto
        {
            PayrollPeriodId = payrollPeriodId,
            DeletedPayrolls = payrolls.Count,
            RevertedInstallments = installments.Count,
            ReopenedLoans = reopenedLoans.Count
        };
    }
}

using Microsoft.EntityFrameworkCore;

public class DeleteLoanService : IDeleteLoanService
{
    private readonly RecepcionDbContext _context;

    public DeleteLoanService(RecepcionDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteLoanResult> DeleteLoanAsync(long loanId)
    {
        var loan = await _context.EmployeeLoans
            .AsNoTracking()
            .Include(x => x.Installments)
            .FirstOrDefaultAsync(x => x.Id == loanId);
        if (loan == null)
        {
            return new DeleteLoanResult(DeleteLoanStatus.LoanNotFound);
        }

        // Si alguna cuota ya fue descontada en planilla, el préstamo forma parte del histórico y no se elimina.
        if (LoanModificationGuard.HasCollectedInstallments(loan))
        {
            return new DeleteLoanResult(DeleteLoanStatus.HasCollectedInstallments);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Se borran en BD solo las cuotas pendientes. Si una planilla cobró alguna cuota después de la
        // verificación anterior, el conteo no coincide y no se elimina nada.
        var deletedInstallments = await _context.EmployeeLoanInstallments.CountAsync(x => x.EmployeeLoanId == loanId);
        if (!await LoanModificationGuard.TryDeletePendingInstallmentsAsync(_context, loanId))
        {
            await transaction.RollbackAsync();
            return new DeleteLoanResult(DeleteLoanStatus.HasCollectedInstallments);
        }

        await _context.EmployeeLoans
            .Where(x => x.Id == loanId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();

        return new DeleteLoanResult(DeleteLoanStatus.Deleted, new DeleteLoanResponseDto
        {
            Id = loanId,
            DeletedInstallments = deletedInstallments
        });
    }
}

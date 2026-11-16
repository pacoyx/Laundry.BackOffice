using Microsoft.EntityFrameworkCore;

public static class LoanModificationGuard
{
    public const string HasCollectedInstallmentsMessage =
        "No se puede modificar ni eliminar el préstamo/adelanto porque ya tiene cuotas cobradas en planilla.";

    /// <summary>
    /// Indica si el préstamo ya tuvo algún cobro: una cuota pagada o vinculada a una planilla. También se considera
    /// cobrado si está cerrado o si su saldo es menor al monto (datos anteriores al manejo de cuotas).
    /// Requiere que las cuotas estén cargadas.
    /// </summary>
    public static bool HasCollectedInstallments(EmployeeLoan loan) =>
        loan.Installments.Any(i => i.IsPaid || i.PayrollId != null)
        || loan.IsClosed
        || loan.Balance < loan.Amount;

    /// <summary>
    /// Borra directamente en BD las cuotas pendientes (no pagadas y sin planilla) del préstamo y verifica que eran
    /// todas sus cuotas. Devuelve false si alguna cuota fue cobrada mientras tanto (p. ej. por una generación de
    /// planilla concurrente); en ese caso el llamador debe hacer rollback. Debe ejecutarse dentro de una transacción.
    /// </summary>
    public static async Task<bool> TryDeletePendingInstallmentsAsync(RecepcionDbContext context, long loanId)
    {
        var total = await context.EmployeeLoanInstallments
            .CountAsync(x => x.EmployeeLoanId == loanId);

        var deleted = await context.EmployeeLoanInstallments
            .Where(x => x.EmployeeLoanId == loanId && !x.IsPaid && x.PayrollId == null)
            .ExecuteDeleteAsync();

        return deleted == total;
    }
}

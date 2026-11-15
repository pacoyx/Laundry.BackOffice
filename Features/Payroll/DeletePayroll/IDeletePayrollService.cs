public interface IDeletePayrollService
{
    /// <summary>
    /// Elimina las planillas generadas del periodo (para regenerar). Conserva el periodo y sus descuentos.
    /// </summary>
    Task<DeletePayrollResult> DeletePayrollAsync(long payrollPeriodId);

    /// <summary>
    /// Elimina el periodo completo: planillas generadas (revirtiendo cuotas), descuentos registrados y el periodo.
    /// </summary>
    Task<DeletePayrollResult> DeletePeriodAsync(long payrollPeriodId);
}

public interface IDeleteLoanService
{
    /// <summary>
    /// Elimina un préstamo/adelanto y sus cuotas, siempre que ninguna cuota haya sido cobrada.
    /// </summary>
    Task<DeleteLoanResult> DeleteLoanAsync(long loanId);
}

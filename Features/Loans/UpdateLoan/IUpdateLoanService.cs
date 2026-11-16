public interface IUpdateLoanService
{
    /// <summary>
    /// Edita fecha, monto, observación y número de cuotas de un préstamo/adelanto sin cuotas cobradas,
    /// regenerando su cronograma de cuotas.
    /// </summary>
    Task<UpdateLoanResult> UpdateLoanAsync(long loanId, RegisterLoanRequestDto request);
}

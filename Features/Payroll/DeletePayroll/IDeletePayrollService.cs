public interface IDeletePayrollService
{
    Task<DeletePayrollResult> DeletePayrollAsync(long payrollPeriodId);
}

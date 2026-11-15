public interface IPayrollDiscountService
{
    Task<IEnumerable<PayrollDiscountDto>?> ListAsync(long payrollPeriodId, long? employeeId);
    Task<PayrollDiscountResult> AddAsync(long payrollPeriodId, CreatePayrollDiscountRequestDto request);
    Task<PayrollDiscountResult> DeleteAsync(long payrollPeriodId, long discountId);
}

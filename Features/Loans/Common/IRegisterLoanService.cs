public interface IRegisterLoanService
{
    Task<RegisterLoanResult> RegisterLoanAsync(long employeeId, LoanType loanType, RegisterLoanRequestDto request);
}

public enum RegisterLoanStatus
{
    Ok,
    EmployeeNotFound,
    EmployeeAdHonorem
}

public record RegisterLoanResult(RegisterLoanStatus Status, EmployeeLoanResponseDto? Data = null);

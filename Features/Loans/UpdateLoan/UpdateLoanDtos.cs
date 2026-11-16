public enum UpdateLoanStatus
{
    Ok,
    LoanNotFound,
    HasCollectedInstallments,
    EmployeeAdHonorem
}

public record UpdateLoanResult(UpdateLoanStatus Status, EmployeeLoanResponseDto? Data = null);

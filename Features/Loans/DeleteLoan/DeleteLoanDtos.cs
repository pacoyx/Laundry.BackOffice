public enum DeleteLoanStatus
{
    Deleted,
    LoanNotFound,
    HasCollectedInstallments
}

public record DeleteLoanResponseDto
{
    public long Id { get; set; }
    public int DeletedInstallments { get; set; }
}

public record DeleteLoanResult(DeleteLoanStatus Status, DeleteLoanResponseDto? Data = null);

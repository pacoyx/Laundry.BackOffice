public record UpdateBranchSalesDto(
    string Description,
    string Address,
    string Status,
    string Code,
    int DefaultPrinterId,
    bool IsCentral
);

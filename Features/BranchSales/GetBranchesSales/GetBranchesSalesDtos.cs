public record BranchSalesComboResponse(
     int Id,
     string Description,
     string Address,
     string Status,
     string Code,
     int DefaultPrinterId,
     string? PrinterName,
     bool IsCentral
);
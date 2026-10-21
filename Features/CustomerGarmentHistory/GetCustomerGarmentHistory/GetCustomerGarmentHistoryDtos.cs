public record CustomerGarmentHistoryResponseDto(
    int Id,
    int ProductId,
    string ProductName,
    decimal Price,
    string Detail,
    int UsageCount,
    DateTime LastUsedAt
);

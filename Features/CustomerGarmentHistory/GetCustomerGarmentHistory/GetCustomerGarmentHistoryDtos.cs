public record CustomerGarmentHistoryResponseDto(
    int Id,
    int ProductId,
    string ProductName,
    string Detail,
    int UsageCount,
    DateTime LastUsedAt
);

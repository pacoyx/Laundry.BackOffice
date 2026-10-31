public record DisposeFixedAssetRequest(
    FixedAssetStatus Status,
    DateTime DisposalDate,
    string DisposalReason);

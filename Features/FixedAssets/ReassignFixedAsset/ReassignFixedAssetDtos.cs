public record ReassignFixedAssetRequest(
    int NewBranchId,
    int? NewResponsibleUserId,
    string? Reason);

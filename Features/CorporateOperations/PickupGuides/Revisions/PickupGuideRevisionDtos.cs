public record PickupGuideDetailRevisionRequest(
    PickupGuideRevisionActionType ActionType,
    long? PickupGuideDetailId,
    int? ServiceItemId,
    decimal? NewQuantity,
    string Reason
);

public record CreatePickupGuideRevisionRequest(long EmployeeId, string? Observation, List<PickupGuideDetailRevisionRequest> Details);

public record PickupGuideDetailRevisionResponse(
    long Id,
    PickupGuideRevisionActionType ActionType,
    long PickupGuideDetailId,
    string ServiceItemName,
    decimal OriginalQuantity,
    decimal NewQuantity,
    string Reason
);

public record PickupGuideRevisionPhotoResponse(
    long Id,
    long PickupGuideRevisionId,
    string FileName,
    string? OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt
);

public record PickupGuideRevisionResponse(
    long Id,
    long PickupGuideId,
    long EmployeeId,
    string EmployeeName,
    string Observation,
    DateTime RevisionDate,
    List<PickupGuideDetailRevisionResponse> Details,
    List<PickupGuideRevisionPhotoResponse> Photos
);

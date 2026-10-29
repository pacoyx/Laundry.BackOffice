public record ItemInspectionResponse(
    long Id,
    int WorkGuideDetailId,
    bool HasStains,
    bool HasTears,
    bool HasFading,
    bool HasMissingParts,
    string? Notes,
    DateTime CreatedAt
);

public record ItemPhotoResponse(
    long Id,
    int WorkGuideMainId,
    string FileName,
    string? OriginalFileName,
    string ContentType,
    long FileSize,
    WorkGuideEvidenceStage Stage,
    long? EmployeeId,
    DateTime CreatedAt
);

public static class PickupGuideRevisionsEndpoints
{
    internal static PickupGuideDetailRevisionResponse ToDetailResponse(PickupGuideDetailRevision detailRevision) => new(
        detailRevision.Id, detailRevision.ActionType, detailRevision.PickupGuideDetailId, detailRevision.PickupGuideDetail.ServiceItem.Name,
        detailRevision.OriginalQuantity, detailRevision.NewQuantity, detailRevision.Reason
    );

    internal static PickupGuideRevisionPhotoResponse ToPhotoResponse(PickupGuideRevisionPhoto photo) => new(
        photo.Id, photo.PickupGuideRevisionId, photo.FileName, photo.OriginalFileName, photo.ContentType, photo.FileSize, photo.CreatedAt
    );

    internal static PickupGuideRevisionResponse ToResponse(PickupGuideRevision revision) => new(
        revision.Id, revision.PickupGuideId, revision.EmployeeId,
        $"{revision.Employee.FirstName} {revision.Employee.LastName}".Trim(),
        revision.Observation, revision.RevisionDate,
        revision.DetailRevisions.Select(ToDetailResponse).ToList(),
        revision.Photos.Select(ToPhotoResponse).ToList()
    );
}

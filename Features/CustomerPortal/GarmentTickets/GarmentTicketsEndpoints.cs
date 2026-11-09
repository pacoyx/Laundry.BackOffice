public static class GarmentTicketsEndpoints
{
    internal static GarmentIntakeTicketDetailResponse ToDetailResponse(GarmentIntakeTicketDetail detail) => new(
        detail.Id, detail.WorkerId, detail.Worker.FullName, detail.ServiceItemId, detail.ServiceItem.Name,
        detail.Quantity, detail.Observations
    );

    internal static GarmentIntakeTicketPhotoResponse ToPhotoResponse(GarmentIntakeTicketPhoto photo) => new(
        photo.Id, photo.OriginalFileName, photo.ContentType, photo.FileSize, photo.CreatedAt
    );

    internal static GarmentIntakeTicketResponse ToResponse(GarmentIntakeTicket ticket) => new(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Status,
        ticket.Created,
        ticket.CreatedByCustomerUser.FullName,
        ticket.VoidedReason,
        ticket.VoidedAt,
        ticket.VoidedByCustomerUser?.FullName,
        ticket.PickupGuideId,
        ticket.PickedUpAt,
        ticket.Details.Select(ToDetailResponse).ToList(),
        ticket.Photos.Select(ToPhotoResponse).ToList()
    );

    internal static GarmentIntakeTicketSummaryResponse ToSummaryResponse(GarmentIntakeTicket ticket) => new(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Status,
        ticket.Created,
        ticket.Details.Sum(d => d.Quantity),
        ticket.Details.Count
    );
}

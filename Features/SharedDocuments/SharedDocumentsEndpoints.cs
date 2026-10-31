public static class SharedDocumentsEndpoints
{
    public static void MapSharedDocuments(this IEndpointRouteBuilder app)
    {
        // Nivel de grupo: solo exige estar autenticado. Los endpoints de
        // administracion (crear/listar lo subido/borrar) exigen ademas el
        // permiso SharedDocuments de forma individual, porque "/my" y
        // "/{id}/file" deben quedar disponibles para CUALQUIER usuario logueado
        // (ej. un chofer en Pickup Mobile viendo lo que le compartieron).
        var group = app.MapGroup("/api/shared-documents")
            .WithTags("SharedDocuments")
            .RequireAuthorization();

        group.MapCreateSharedDocument();
        group.MapGetSharedDocuments();
        group.MapGetMySharedDocuments();
        group.MapGetSharedDocumentFile();
        group.MapDeleteSharedDocument();
    }

    internal static SharedDocumentResponse ToResponse(SharedDocument doc) => new(
        doc.Id,
        doc.Title,
        doc.Category,
        doc.Notes,
        doc.FileName,
        doc.ContentType,
        doc.CreatedAt,
        doc.UploadedByUserId,
        doc.UploadedByUser.Name,
        doc.Recipients.Select(r => new SharedDocumentRecipientResponse(r.UserId, r.User.Name)).ToList()
    );
}

public record SharedDocumentResponse(
    long Id,
    string Title,
    string Category,
    string? Notes,
    string FileName,
    string ContentType,
    DateTime CreatedAt,
    int UploadedByUserId,
    string UploadedByUserName,
    List<SharedDocumentRecipientResponse> Recipients);

public record SharedDocumentRecipientResponse(int UserId, string UserName);

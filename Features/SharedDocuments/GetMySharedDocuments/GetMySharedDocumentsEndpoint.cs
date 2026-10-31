using Microsoft.EntityFrameworkCore;

public static class GetMySharedDocumentsEndpoint
{
    public static void MapGetMySharedDocuments(this IEndpointRouteBuilder app)
    {
        app.MapGet("/my", async (System.Security.Claims.ClaimsPrincipal principal, RecepcionDbContext db, CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var documents = await db.SharedDocuments
                .AsNoTracking()
                .Include(x => x.UploadedByUser)
                .Include(x => x.Recipients).ThenInclude(r => r.User)
                .Where(x => x.Recipients.Any(r => r.UserId == userId.Value))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return Results.Ok(new ApiResponse<List<SharedDocumentResponse>>
            {
                Success = true,
                Message = "Documentos compartidos contigo.",
                StatusCode = StatusCodes.Status200OK,
                Data = documents.Select(SharedDocumentsEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetMySharedDocuments")
        .Produces<ApiResponse<List<SharedDocumentResponse>>>(StatusCodes.Status200OK);
    }
}

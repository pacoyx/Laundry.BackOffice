using Microsoft.EntityFrameworkCore;

public static class GetSharedDocumentsEndpoint
{
    public static void MapGetSharedDocuments(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (System.Security.Claims.ClaimsPrincipal principal, RecepcionDbContext db, CancellationToken cancellationToken) =>
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
                .Where(x => x.UploadedByUserId == userId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return Results.Ok(new ApiResponse<List<SharedDocumentResponse>>
            {
                Success = true,
                Message = "Documentos que compartiste.",
                StatusCode = StatusCodes.Status200OK,
                Data = documents.Select(SharedDocumentsEndpoints.ToResponse).ToList()
            });
        })
        .RequirePermission(PermissionCodes.SharedDocuments)
        .WithName("GetSharedDocuments")
        .Produces<ApiResponse<List<SharedDocumentResponse>>>(StatusCodes.Status200OK);
    }
}

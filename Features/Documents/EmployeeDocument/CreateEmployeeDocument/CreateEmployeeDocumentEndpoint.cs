using Microsoft.AspNetCore.Mvc;

public static class CreateEmployeeDocumentEndpoint
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "application/pdf"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public static RouteGroupBuilder MapCreateEmployeeDocument(this RouteGroupBuilder group)
    {
        group.MapPost("/{employeeId:long}/documents", async (
            long employeeId,
            [FromForm] int documentTypeId,
            [FromForm] string? notes,
            IFormFile file,
            ICreateEmployeeDocumentService service,
            CancellationToken cancellationToken) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "El archivo está vacío."
                });
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG, WEBP y PDF."
                });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "El archivo supera el tamaño máximo permitido de 10 MB."
                });
            }

            var result = await service.CreateEmployeeDocumentAsync(employeeId, documentTypeId, notes, file, cancellationToken);

            if (result.Error == CreateEmployeeDocumentError.EmployeeNotFound)
            {
                return Results.NotFound(new ApiResponse<EmployeeDocumentResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Trabajador con ID {employeeId} no encontrado."
                });
            }

            if (result.Error == CreateEmployeeDocumentError.DocumentTypeNotFound)
            {
                return Results.BadRequest(new ApiResponse<EmployeeDocumentResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Tipo de documento con ID {documentTypeId} no encontrado."
                });
            }

            return Results.Ok(new ApiResponse<EmployeeDocumentResponseDto>
            {
                Success = true,
                Data = result.Document,
                StatusCode = StatusCodes.Status201Created,
                Message = "Documento registrado correctamente"
            });
        })
        .DisableAntiforgery()
        .WithName("CreateEmployeeDocument")
        .Produces<ApiResponse<EmployeeDocumentResponseDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}

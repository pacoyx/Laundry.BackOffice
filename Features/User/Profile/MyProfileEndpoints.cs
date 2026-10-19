using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

public static class MyProfileEndpoints
{
    private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedDocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "application/pdf"
    };

    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024;

    public static void MapMyProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", async (ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            var response = new ApiResponse<MyProfileResponseDto>
            {
                Success = true,
                Message = "Perfil obtenido correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new MyProfileResponseDto(
                    user.Id,
                    user.Name,
                    user.UserName,
                    user.Email,
                    user.Phone,
                    user.Role,
                    user.Status,
                    user.PhotoUrl)
            };

            return Results.Ok(response);
        })
        .WithName("GetMyProfile")
        .Produces<ApiResponse<MyProfileResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapPut("/me", async (
            ClaimsPrincipal principal,
            UpdateMyProfileRequestDto request,
            IValidator<UpdateMyProfileRequestDto> validator,
            RecepcionDbContext db) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.Name = request.Name;
            user.Email = request.Email;
            user.Phone = request.Phone;
            await db.SaveChangesAsync();

            var response = new ApiResponse<MyProfileResponseDto>
            {
                Success = true,
                Message = "Datos actualizados correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new MyProfileResponseDto(
                    user.Id,
                    user.Name,
                    user.UserName,
                    user.Email,
                    user.Phone,
                    user.Role,
                    user.Status,
                    user.PhotoUrl)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateMyProfile")
        .Produces<ApiResponse<MyProfileResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapPost("/me/password", async (
            ClaimsPrincipal principal,
            ChangeMyPasswordRequestDto request,
            IValidator<ChangeMyPasswordRequestDto> validator,
            IEncryptService encryptService,
            RecepcionDbContext db) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            var currentPasswordValid = encryptService.VerifyPassword(
                request.CurrentPassword, $"{user.HashPassword}:{user.Password}");
            if (!currentPasswordValid)
            {
                return Results.BadRequest(new[] { "La contraseña actual es incorrecta." });
            }

            var keyPwh = encryptService.HashPassword(request.NewPassword);
            user.HashPassword = keyPwh.Split(":")[0];
            user.Password = keyPwh.Split(":")[1];
            user.RefreshToken = "";
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Contraseña actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("ChangeMyPassword")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapPost("/me/photo", async (
            ClaimsPrincipal principal,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!AllowedPhotoContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new[] { "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG y WEBP." });
            }

            if (file.Length > MaxPhotoSizeBytes)
            {
                return Results.BadRequest(new[] { "El archivo supera el tamaño máximo permitido de 5 MB." });
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return Results.NotFound();
            }

            var previousPhotoUrl = user.PhotoUrl;

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("Users", "Photos", userId.Value, file.FileName, file.ContentType, stream, cancellationToken);

            user.PhotoUrl = uploadResult.FilePath;
            await db.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(previousPhotoUrl))
            {
                await storage.DeleteAsync(previousPhotoUrl, cancellationToken);
            }

            var response = new ApiResponse<MyPhotoResponseDto>
            {
                Success = true,
                Message = "Foto actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new MyPhotoResponseDto(user.PhotoUrl)
            };

            return Results.Ok(response);
        })
        .DisableAntiforgery()
        .WithName("UpdateMyPhoto")
        .Produces<ApiResponse<MyPhotoResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapGet("/me/photo", async (
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null || string.IsNullOrEmpty(user.PhotoUrl))
            {
                return Results.NotFound();
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(user.PhotoUrl, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var stream = await storage.OpenReadAsync(user.PhotoUrl, cancellationToken);
            return Results.File(stream, contentType, enableRangeProcessing: true);
        })
        .WithName("GetMyPhoto")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapDelete("/me/photo", async (
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                await storage.DeleteAsync(user.PhotoUrl, cancellationToken);
            }

            user.PhotoUrl = "";
            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Foto eliminada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("DeleteMyPhoto")
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapPost("/me/documents", async (
            ClaimsPrincipal principal,
            [FromForm] string documentName,
            [FromForm] string? notes,
            IFormFile file,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(documentName) || documentName.Length > 100)
            {
                return Results.BadRequest(new[] { "El nombre del documento es obligatorio y debe tener máximo 100 caracteres." });
            }

            if (file.Length == 0)
            {
                return Results.BadRequest(new[] { "El archivo está vacío." });
            }

            if (!AllowedDocumentContentTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new[] { "Tipo de archivo no permitido. Formatos aceptados: JPG, PNG, WEBP y PDF." });
            }

            if (file.Length > MaxDocumentSizeBytes)
            {
                return Results.BadRequest(new[] { "El archivo supera el tamaño máximo permitido de 10 MB." });
            }

            var userExists = await db.Users.AnyAsync(x => x.Id == userId, cancellationToken);
            if (!userExists)
            {
                return Results.NotFound();
            }

            await using var stream = file.OpenReadStream();
            var uploadResult = await storage.UploadAsync("Users", "Documents", userId.Value, file.FileName, file.ContentType, stream, cancellationToken);

            var document = new UserDocument
            {
                UserId = userId.Value,
                DocumentName = documentName,
                FileName = file.FileName,
                BlobUrl = uploadResult.FilePath,
                ContentType = file.ContentType,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            await db.UserDocuments.AddAsync(document);
            await db.SaveChangesAsync(cancellationToken);

            var response = new ApiResponse<MyDocumentResponseDto>
            {
                Success = true,
                Message = "Documento registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = new MyDocumentResponseDto(
                    document.Id,
                    document.DocumentName,
                    document.FileName,
                    document.BlobUrl,
                    document.ContentType,
                    document.Notes,
                    document.CreatedAt)
            };

            return Results.Created($"/api/user/me/documents/{document.Id}", response);
        })
        .DisableAntiforgery()
        .WithName("CreateMyDocument")
        .Produces<ApiResponse<MyDocumentResponseDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapGet("/me/documents", async (ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var documents = await db.UserDocuments
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new MyDocumentResponseDto(
                    x.Id, x.DocumentName, x.FileName, x.BlobUrl, x.ContentType, x.Notes, x.CreatedAt))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<MyDocumentResponseDto>>
            {
                Success = true,
                Message = "Documentos obtenidos correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = documents
            });
        })
        .WithName("GetMyDocuments")
        .Produces<ApiResponse<List<MyDocumentResponseDto>>>(StatusCodes.Status200OK)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapGet("/me/documents/{id:long}/download", async (
            long id,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var document = await db.UserDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
            if (document is null)
            {
                return Results.NotFound();
            }

            var stream = await storage.OpenReadAsync(document.BlobUrl, cancellationToken);
            return Results.File(stream, document.ContentType, document.FileName, enableRangeProcessing: true);
        })
        .WithName("DownloadMyDocument")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("UserProfile")
        .RequireAuthorization();

        app.MapDelete("/me/documents/{id:long}", async (
            long id,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var document = await db.UserDocuments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
            if (document is null)
            {
                return Results.NotFound();
            }

            db.UserDocuments.Remove(document);
            await db.SaveChangesAsync(cancellationToken);

            await storage.DeleteAsync(document.BlobUrl, cancellationToken);

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Documento eliminado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("DeleteMyDocument")
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("UserProfile")
        .RequireAuthorization();
    }
}

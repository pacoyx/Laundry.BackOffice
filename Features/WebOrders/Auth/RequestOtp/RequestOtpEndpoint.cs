using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public static class RequestOtpEndpoint
{
    public static void MapRequestOtp(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp/request", async (
            RequestOtpRequest request,
            RecepcionDbContext db,
            IEncryptService encryptService,
            ISmsService smsService,
            IOptions<WebOrdersSettings> settings) =>
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El celular es obligatorio.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var config = settings.Value;
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            var lastOtp = await db.WebCustomerOtps
                .Where(x => x.Phone == request.Phone)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastOtp != null && (now - lastOtp.CreatedAt).TotalSeconds < config.OtpResendCooldownSeconds)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Espera unos segundos antes de solicitar otro código.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var requestsLastHour = await db.WebCustomerOtps
                .CountAsync(x => x.Phone == request.Phone && x.CreatedAt >= now.AddHours(-1));

            if (requestsLastHour >= config.OtpMaxRequestsPerHour)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Alcanzaste el límite de códigos solicitados. Intenta más tarde.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");

            db.WebCustomerOtps.Add(new WebCustomerOtp
            {
                Phone = request.Phone,
                CodeHash = encryptService.HashPassword(code),
                ExpiresAt = now.AddMinutes(config.OtpExpirationMinutes),
                AttemptCount = 0,
                IsUsed = false,
                CreatedAt = now
            });

            await db.SaveChangesAsync();

            await smsService.SendAsync(request.Phone, $"Tu código de verificación es {code}. Vence en {config.OtpExpirationMinutes} minutos.");

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Código enviado por SMS.",
                StatusCode = StatusCodes.Status200OK
            });
        })
        .WithName("RequestWebCustomerOtp")
        .WithTags("WebOrders")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}

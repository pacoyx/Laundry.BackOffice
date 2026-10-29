using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public static class VerifyOtpEndpoint
{
    public static void MapVerifyOtp(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/otp/verify", async (
            VerifyOtpRequest request,
            RecepcionDbContext db,
            IEncryptService encryptService,
            JwtService jwtService,
            IOptions<WebOrdersSettings> settings) =>
        {
            var config = settings.Value;
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            var otp = await db.WebCustomerOtps
                .Where(x => x.Phone == request.Phone && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp is null)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Solicita un código primero.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (otp.ExpiresAt < now)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El código expiró, solicita uno nuevo.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (otp.AttemptCount >= config.OtpMaxAttempts)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Demasiados intentos. Solicita un código nuevo.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!encryptService.VerifyPassword(request.Code, otp.CodeHash))
            {
                otp.AttemptCount++;
                await db.SaveChangesAsync();

                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Código incorrecto.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var account = await db.WebCustomerAccounts
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Phone == request.Phone);

            Customer customer;

            if (account != null)
            {
                customer = account.Customer;

                if (!account.IsActive)
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Tu cuenta se encuentra inactiva.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }
            }
            else
            {
                var existingCustomer = await db.Customers.FirstOrDefaultAsync(x => x.Phone == request.Phone);

                if (existingCustomer != null)
                {
                    customer = existingCustomer;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.Name) || request.DistrictId is null)
                    {
                        return Results.BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Para registrarte, ingresa tu nombre y distrito.",
                            StatusCode = StatusCodes.Status400BadRequest,
                            Errors = new List<string> { "requires_registration" }
                        });
                    }

                    var districtExists = await db.Districts.AnyAsync(x => x.Id == request.DistrictId);
                    if (!districtExists)
                    {
                        return Results.BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "El distrito indicado no existe.",
                            StatusCode = StatusCodes.Status400BadRequest
                        });
                    }

                    customer = new Customer
                    {
                        FirtsName = request.Name!,
                        Phone = request.Phone,
                        DistrictId = request.DistrictId,
                        CustomerType = CustomerType.Individual,
                        Origin = CustomerOrigin.WebLanding,
                        Status = "A",
                        IsActive = true
                    };

                    db.Customers.Add(customer);
                    await db.SaveChangesAsync();
                }

                account = new WebCustomerAccount
                {
                    CustomerId = customer.Id,
                    Customer = customer,
                    Phone = request.Phone,
                    IsPhoneVerified = true,
                    IsActive = true,
                    CreatedAt = now
                };

                db.WebCustomerAccounts.Add(account);
            }

            otp.IsUsed = true;

            // Si la cuenta/cliente son nuevos, aún no tienen Id hasta que se guarden.
            await db.SaveChangesAsync();

            var token = jwtService.GenerateWebCustomerToken(account.Id, account.CustomerId, request.Phone);
            var refreshToken = jwtService.GenerateRefreshToken();

            account.RefreshToken = refreshToken;
            account.LastLoginAt = now;
            account.IsPhoneVerified = true;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<VerifyOtpResponse>
            {
                Success = true,
                Message = "Inicio de sesión correcto.",
                StatusCode = StatusCodes.Status200OK,
                Data = new VerifyOtpResponse(
                    token,
                    refreshToken,
                    new WebCustomerMeResponse(customer.Id, customer.FirtsName, customer.DistrictId))
            });
        })
        .WithName("VerifyWebCustomerOtp")
        .WithTags("WebOrders")
        .Produces<ApiResponse<VerifyOtpResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}

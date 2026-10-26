
using Microsoft.EntityFrameworkCore;

public static class LoginUserEndpoint
{
    public static void MapLoginUser(this IEndpointRouteBuilder app)
    {

        app.MapPost("/login", async (
                LoginUserDto loginDto, 
                RecepcionDbContext context, 
                JwtService jwtService, 
                IAppLogger<string> logger, IEncryptService HashingService) =>
        {
            if (context.Users == null)
            {
                return Results.Problem("Users table is not available", statusCode: 500);
            }


            var user = await context.Users
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.UserName == loginDto.Username );
            if (user == null)
            {
                logger.LogInformacion("Usuario no encontrado: " + loginDto.Username);
                return Results.Unauthorized();
            }
            

            var passwordhHash = HashingService.VerifyPassword(loginDto.Password, $"{user.HashPassword}:{user.Password}");
            if (!passwordhHash)
            {
                logger.LogWarning("Usuario fallo login password: " + loginDto.Username);
                return Results.Unauthorized();
            }


            // var user = await context.Users
            //                         .AsNoTracking()
            //                         .FirstOrDefaultAsync(u => u.UserName == loginDto.Username && u.Password == loginDto.Password);            
            // if (user == null)
            // {
            //     logger.LogWarning("Usuario fallo login: " + loginDto.Username);
            //     return Results.Unauthorized();
            // }

            var branchSalesUser = await context.BrachSalesUsers
                                            .Include(bsu => bsu.BranchSales)
                                            .AsNoTracking()
                                            .Where(bsu => bsu.UserId == user.Id && bsu.Status == "A")
                                            .ToListAsync();

            if (branchSalesUser == null)
            {
                logger.LogWarning("Usuario no tiene asignado ninguna sucursal de ventas: " + loginDto.Username);
                return Results.Problem("User does not have any branch sales assigned", statusCode: 401);
            }

            var roles = await context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var permissions = await context.RolePermissions
                .Where(rp => context.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            var accessWindows = await context.UserAccessSchedules
                .Where(x => x.UserId == user.Id && x.IsActive)
                .Select(x => new AccessScheduleWindow(x.DayOfWeek, x.StartTime, x.EndTime))
                .ToListAsync();

            if (!AccessScheduleEvaluator.IsWithinSchedule(accessWindows, PeruClock.Now))
            {
                logger.LogWarning("Usuario fuera de su horario de acceso permitido: " + loginDto.Username);
                return Results.Json(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Fuera de su horario de acceso permitido.",
                    StatusCode = StatusCodes.Status403Forbidden,
                    Data = ""
                }, statusCode: StatusCodes.Status403Forbidden);
            }

            var token = jwtService.GenerateToken(user.Id.ToString(), loginDto.Username, roles, permissions, user.EmployeeId, accessWindows);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            context.Users.Update(user);
            await context.SaveChangesAsync();


            var loginUserResponseDto = new LoginUserResponseDto(
                token,
                refreshToken,
                user.Name,
                user.UserName,
                user.Role,
                user.Status,
                user.Id,
                branchSalesUser.Select(bsu => new
                BranchSalesResponseDto(
                    bsu.Id,
                    bsu.BranchSalesId,
                    bsu.BranchSales!.Description,
                    bsu.Status
                    )).ToList(),
                roles,
                permissions
            );

            var response = new ApiResponse<LoginUserResponseDto>()
            {
                Success = true,
                Message = "Request was successful",
                Data = loginUserResponseDto,
                StatusCode = 200
            };
            
            logger.LogInformacion("User logueado correctamente: " + loginDto.Username);
            return Results.Ok(response);
        }).AllowAnonymous();


        app.MapPost("/refresh-token", async (
            RefreshTokenRequestDto refreshTokenRequest, 
            RecepcionDbContext context, 
            JwtService jwtService,
            IAppLogger<string> logger) =>
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(refreshTokenRequest.Token);            
            var username = principal.Identity?.Name;
            if (username == null)
            {
                logger.LogWarning("RefreshToken: Token no autorizado: " + refreshTokenRequest.Token);
                return Results.Unauthorized();
            }

            var user = await context.Users
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.UserName == username);
            

            if (user == null || user.RefreshToken != refreshTokenRequest.RefreshToken)
            {
                // Console.WriteLine("Debugueando ERROR ======");
                // Console.WriteLine("No autorizado ======");
                // Console.WriteLine("User: ===>" + Newtonsoft.Json.JsonConvert.SerializeObject(user));
                logger.LogWarning("RefreshToken: Usuario no encontrado por username: " + refreshTokenRequest.Token);
                return Results.Unauthorized();
            }

            var roles = await context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var permissions = await context.RolePermissions
                .Where(rp => context.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            var accessWindows = await context.UserAccessSchedules
                .Where(x => x.UserId == user.Id && x.IsActive)
                .Select(x => new AccessScheduleWindow(x.DayOfWeek, x.StartTime, x.EndTime))
                .ToListAsync();

            if (!AccessScheduleEvaluator.IsWithinSchedule(accessWindows, PeruClock.Now))
            {
                logger.LogWarning("RefreshToken: Usuario fuera de su horario de acceso permitido: " + user.UserName);
                return Results.Json(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Fuera de su horario de acceso permitido.",
                    StatusCode = StatusCodes.Status403Forbidden,
                    Data = ""
                }, statusCode: StatusCodes.Status403Forbidden);
            }

            var newJwtToken = jwtService.GenerateToken(user.Id.ToString(), user.UserName, roles, permissions, user.EmployeeId, accessWindows);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            context.Users.Update(user);
            await context.SaveChangesAsync();

            var response = new
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                Roles = roles,
                Permissions = permissions
            };

            return Results.Ok(response);
        });

    }
}
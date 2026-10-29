using Microsoft.EntityFrameworkCore;

public static class CreateUserEndpoint
{
    public static void MapCreateUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (CreateUserDto userDto, ICreateUserService userService, RecepcionDbContext db) =>
        {
            if (userDto == null)
            {
                return Results.BadRequest();
            }

            var existUser = await userService.ExistUser(userDto.UserName);
            if (existUser)
            {
                var api = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "User already exists",
                    StatusCode = 400,
                    Success = false
                };

                return Results.Ok(api);
            }

            if (userDto.EmployeeId.HasValue)
            {
                var employeeExists = await db.Employees.AnyAsync(x => x.Id == userDto.EmployeeId.Value);
                if (!employeeExists)
                {
                    return Results.Ok(new ApiResponse<string>
                    {
                        Data = "",
                        Message = "El empleado indicado no existe",
                        StatusCode = 400,
                        Success = false
                    });
                }
            }

            var user = await userService.Create(userDto);
            var responseUser = new CreateUserResponseDto(user.Id, user.Name, user.UserName, user.Role);
            var response = new ApiResponse<string>()
            {                
                Data = responseUser.id.ToString(),
                Message = "User created successfully",
                StatusCode = 201,
                Success = true
            };
            return Results.Ok(response);
        }).RequireAuthorization();
    }
}
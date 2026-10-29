using Microsoft.EntityFrameworkCore;

public static class UpdateUserEndpoint{

    public static void MapUpdateUser(this IEndpointRouteBuilder app){

        app.MapPut("/{id}", async (int id, UpdateUserRequestDto userDto, IUpdateUserService userService, RecepcionDbContext db) =>
        {
            if (userDto == null)
            {
                return Results.BadRequest();
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

            var user = await userService.UpdateUser(id,userDto);
            if (user == null)
            {
                var api = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "User not found",
                    StatusCode = 404,
                    Success = false
                };

                return Results.Ok(api);
            }

            var response = new ApiResponse<string>()
            {
                Data = "",
                Message = "User updated successfully",
                StatusCode = 200,
                Success = true
            };
            return Results.Ok(response);
        });
    }
}
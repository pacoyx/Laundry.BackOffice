public static class GetUserEndpoint
{
    public static void MapGetUser(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (int id, IDeleteUserService userService) =>
        {
            var user = await userService.GetUser(id);
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

            var response = new ApiResponse<GetUserResponseDto>()
            {
                Data = new GetUserResponseDto
                (
                    user.Id,
                    user.Name,
                    user.UserName,
                    user.Email,
                    user.Role,
                    user.Status,
                    user.EmployeeId
                ),
                Message = "User found",
                StatusCode = 200,
                Success = true
            };
            return Results.Ok(response);            
        }).RequireAuthorization();
    }
}
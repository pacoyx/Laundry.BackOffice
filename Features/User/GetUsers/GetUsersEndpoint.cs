public static class GetUsersEndpoint
{
    public static void MapGetUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (IGetUsersService userService) =>
        {
            var users = await userService.GetUsers();
            var usersResponse = users.Select(user => new GetUsersResponseDto
            (
                user.Id,
                user.Name,
                user.UserName,
                user.Email,
                user.Role,
                user.Status,
                user.EmployeeId
            )).ToList();

            var response = new ApiResponse<List<GetUsersResponseDto>>()
            {
                Data = usersResponse,
                Message = "Users found",
                StatusCode = 200,
                Success = true
            };
            return Results.Ok(response);
        }).RequireAuthorization();
    }
}
using Microsoft.EntityFrameworkCore;

public static class GetUserAccessScheduleEndpoint
{
    public static void MapGetUserAccessSchedule(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}/access-schedule", async (int id, RecepcionDbContext db) =>
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                return Results.NotFound("Usuario no encontrado.");
            }

            var windows = await db.UserAccessSchedules
                .AsNoTracking()
                .Where(x => x.UserId == id)
                .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
                .Select(x => new AccessScheduleWindowResponse(x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.IsActive))
                .ToListAsync();

            var response = new UserAccessScheduleResponse(user.Id, user.UserName, windows);

            return Results.Ok(new ApiResponse<UserAccessScheduleResponse>
            {
                Success = true,
                Message = "Horario de acceso del usuario.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }
}

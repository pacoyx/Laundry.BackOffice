using Microsoft.EntityFrameworkCore;

public static class UpdateUserAccessScheduleEndpoint
{
    public static void MapUpdateUserAccessSchedule(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}/access-schedule", async (
            int id,
            UpdateUserAccessScheduleRequest request,
            RecepcionDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                return Results.NotFound("Usuario no encontrado.");
            }

            if (request.Windows.Any(w => w.StartTime >= w.EndTime))
            {
                return Results.BadRequest("La hora de inicio debe ser menor a la hora de fin en cada rango.");
            }

            var currentWindows = await db.UserAccessSchedules
                .Where(x => x.UserId == id)
                .ToListAsync();

            db.UserAccessSchedules.RemoveRange(currentWindows);

            foreach (var window in request.Windows)
            {
                db.UserAccessSchedules.Add(new UserAccessSchedule
                {
                    UserId = id,
                    DayOfWeek = window.DayOfWeek,
                    StartTime = window.StartTime,
                    EndTime = window.EndTime,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();

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
                Message = "Horario de acceso del usuario actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }
}

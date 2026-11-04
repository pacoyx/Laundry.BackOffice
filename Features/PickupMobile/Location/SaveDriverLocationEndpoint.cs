using System.Security.Claims;
using _24dex_backend_comercial.Features.DriverTracking;
using Microsoft.EntityFrameworkCore;

public static class SaveDriverLocationEndpoint
{
    public static void MapSaveDriverLocation(this IEndpointRouteBuilder app)
    {
        app.MapPost("/location", async (
            SaveDriverLocationRequest request,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IDriverLocationBroadcaster broadcaster) =>
        {
            var employeeId = principal.GetEmployeeId();
            if (employeeId is null)
            {
                return Results.BadRequest(new[] { "El usuario no está vinculado a un empleado. Contacte al administrador para habilitar el acceso móvil." });
            }

            var employee = await db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId.Value);

            if (employee is null)
            {
                return Results.NotFound();
            }

            var location = new DriverLocation
            {
                EmployeeId = employeeId.Value,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AccuracyMeters = request.AccuracyMeters,
                RecordedAt = request.RecordedAt,
                CreatedAt = DateTime.UtcNow
            };

            db.DriverLocations.Add(location);
            await db.SaveChangesAsync();

            await broadcaster.BroadcastAsync(new DriverLocationDto(
                employee.Id,
                $"{employee.FirstName} {employee.LastName}".Trim(),
                location.Latitude,
                location.Longitude,
                location.AccuracyMeters,
                location.RecordedAt
            ));

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Ubicación registrada.",
                StatusCode = StatusCodes.Status200OK,
                Data = ""
            });
        })
        .WithName("SaveDriverLocation")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

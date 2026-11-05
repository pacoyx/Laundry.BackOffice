using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.DriverTracking;

// Lista para el combo de la pantalla de "Recorrido por dia": solo choferes que alguna vez
// mandaron su ubicacion (no todos los empleados), para no mostrar gente sin datos que consultar.
public static class GetDriverTrackingDriversEndpoint
{
    public static void MapGetDriverTrackingDrivers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers", async (RecepcionDbContext db) =>
        {
            var drivers = await db.DriverLocations
                .Select(x => x.EmployeeId)
                .Distinct()
                .Join(db.Employees, id => id, e => e.Id, (id, e) => new DriverLookupDto(
                    e.Id,
                    (e.FirstName + " " + e.LastName).Trim()))
                .OrderBy(x => x.EmployeeName)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<DriverLookupDto>>
            {
                Success = true,
                Message = "Choferes con registro de ubicación.",
                StatusCode = StatusCodes.Status200OK,
                Data = drivers
            });
        })
        .WithName("GetDriverTrackingDrivers")
        .Produces<ApiResponse<List<DriverLookupDto>>>(StatusCodes.Status200OK);
    }
}

public record DriverLookupDto(long EmployeeId, string EmployeeName);

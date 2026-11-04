using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.DriverTracking;

public static class GetDriverLocationsEndpoint
{
    public static void MapGetDriverLocations(this IEndpointRouteBuilder app)
    {
        app.MapGet("/locations", async (RecepcionDbContext db) =>
        {
            // Ultima posicion de cada chofer activo: agrupa por EmployeeId y toma la fila con
            // mayor Id de cada grupo (mismo truco que GetWorkGuideByDocumentEndpoint usa para
            // LocationWorkGuide).
            var latestByEmployee = await db.DriverLocations
                .AsNoTracking()
                .Include(x => x.Employee)
                .Where(x => x.Employee.IsActive)
                .GroupBy(x => x.EmployeeId)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToListAsync();

            var response = latestByEmployee.Select(x => new DriverLocationDto(
                x.EmployeeId,
                $"{x.Employee.FirstName} {x.Employee.LastName}".Trim(),
                x.Latitude,
                x.Longitude,
                x.AccuracyMeters,
                x.RecordedAt
            )).ToList();

            return Results.Ok(new ApiResponse<List<DriverLocationDto>>
            {
                Success = true,
                Message = "Última ubicación conocida de cada chofer.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetDriverLocations")
        .Produces<ApiResponse<List<DriverLocationDto>>>(StatusCodes.Status200OK);
    }
}

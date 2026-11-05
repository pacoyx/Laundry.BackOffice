using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.DriverTracking;

// Recorrido de choferes en un dia calendario (hora de Peru), opcionalmente acotado a un rango
// horario. Sin rango, trae el dia completo. Sin employeeId (o vacio), trae a TODOS los choferes
// con registros ese dia -asi la pantalla soporta tanto "un chofer" como "varios" como "todos" sin
// cambiar de endpoint. Cada chofer se ordena por su propio RecordedAt (hora real del GPS en el
// celular), no por Id, para que el trazo refleje el orden real del recorrido aunque algun ping
// haya llegado tarde al servidor por demora de red.
public static class GetDriverRouteEndpoint
{
    public static void MapGetDriverRoute(this IEndpointRouteBuilder app)
    {
        app.MapGet("/route", async (
            long[]? employeeId,
            DateOnly date,
            TimeOnly? startTime,
            TimeOnly? endTime,
            RecepcionDbContext db) =>
        {
            if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value)
            {
                return Results.BadRequest(new[] { "La hora final debe ser posterior a la hora inicial." });
            }

            var localStart = date.ToDateTime(startTime ?? TimeOnly.MinValue);
            var localEnd = endTime.HasValue
                ? date.ToDateTime(endTime.Value)
                : date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var utcStart = PeruClock.ToUtc(localStart);
            var utcEnd = PeruClock.ToUtc(localEnd);

            var query = db.DriverLocations
                .AsNoTracking()
                .Where(x => x.RecordedAt >= utcStart && x.RecordedAt < utcEnd);

            if (employeeId is { Length: > 0 })
            {
                query = query.Where(x => employeeId.Contains(x.EmployeeId));
            }

            var rows = await query
                .OrderBy(x => x.RecordedAt)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.Latitude,
                    x.Longitude,
                    x.AccuracyMeters,
                    x.RecordedAt
                })
                .ToListAsync();

            var employeeIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            var employeeNames = await db.Employees
                .AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            var routes = rows
                .GroupBy(x => x.EmployeeId)
                .Select(g => new DriverRouteResponse(
                    g.Key,
                    employeeNames.TryGetValue(g.Key, out var name) ? name : string.Empty,
                    g.Select(p => new DriverRoutePointDto(p.Latitude, p.Longitude, p.AccuracyMeters, p.RecordedAt)).ToList()))
                .OrderBy(r => r.EmployeeName)
                .ToList();

            return Results.Ok(new ApiResponse<List<DriverRouteResponse>>
            {
                Success = true,
                Message = routes.Count == 0
                    ? "No se encontraron registros de ubicación en ese rango."
                    : "Recorrido obtenido correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = routes
            });
        })
        .WithName("GetDriverRoute")
        .Produces<ApiResponse<List<DriverRouteResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record DriverRoutePointDto(double Latitude, double Longitude, double? AccuracyMeters, DateTime RecordedAt);

public record DriverRouteResponse(long EmployeeId, string EmployeeName, List<DriverRoutePointDto> Points);

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace _24dex_backend_comercial.Features.DriverTracking;

// Corre a diario via Hangfire (ver Program.cs). El mapa solo necesita el recorrido reciente de
// cada chofer, no un historial indefinido, asi que se borra todo lo mas viejo que la ventana de
// retencion configurada en DriverTracking:LocationRetentionDays.
public class DriverLocationCleanupService : IDriverLocationCleanupService
{
    private readonly RecepcionDbContext _db;
    private readonly DriverTrackingSettings _settings;
    private readonly ILogger<DriverLocationCleanupService> _logger;

    public DriverLocationCleanupService(
        RecepcionDbContext db,
        IOptions<DriverTrackingSettings> settings,
        ILogger<DriverLocationCleanupService> logger)
    {
        _db = db;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task CleanupOldLocationsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - TimeSpan.FromDays(_settings.LocationRetentionDays);

        // ExecuteDeleteAsync borra directo en SQL (sin traer filas a memoria), necesario porque
        // esta tabla crece un registro cada ~25s por chofer activo.
        var deleted = await _db.DriverLocations
            .Where(x => x.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation(
            "Limpieza de DriverLocations: {Deleted} registros anteriores a {Cutoff} eliminados.",
            deleted,
            cutoff);
    }
}

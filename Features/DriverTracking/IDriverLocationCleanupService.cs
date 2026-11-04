namespace _24dex_backend_comercial.Features.DriverTracking;

public interface IDriverLocationCleanupService
{
    Task CleanupOldLocationsAsync(CancellationToken cancellationToken = default);
}

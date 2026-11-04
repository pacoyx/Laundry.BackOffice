namespace _24dex_backend_comercial.Features.DriverTracking;

public interface IDriverLocationBroadcaster
{
    Task BroadcastAsync(DriverLocationDto location, CancellationToken cancellationToken = default);
}

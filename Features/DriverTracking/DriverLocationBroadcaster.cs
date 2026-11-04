using Microsoft.AspNetCore.SignalR;

namespace _24dex_backend_comercial.Features.DriverTracking;

public class DriverLocationBroadcaster : IDriverLocationBroadcaster
{
    private readonly IHubContext<DriverTrackingHub> _hubContext;

    public DriverLocationBroadcaster(IHubContext<DriverTrackingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastAsync(DriverLocationDto location, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("DriverLocationUpdated", location, cancellationToken);
    }
}

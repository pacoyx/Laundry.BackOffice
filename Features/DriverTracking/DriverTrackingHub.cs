using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace _24dex_backend_comercial.Features.DriverTracking;

// A diferencia de PrintHub, aqui solo se conectan clientes del backoffice que estan viendo el
// mapa (nunca los choferes -ellos mandan su ubicacion por HTTP normal, ver
// SaveDriverLocationEndpoint). Por eso no hace falta un registry de conexiones ni grupos: basta
// con transmitir a todos los conectados a este hub.
//
// Requiere el mismo permiso que GetDriverLocationsEndpoint: sin esto, cualquiera que conozca la
// URL del hub podria conectarse y recibir la ubicacion en vivo de todos los choferes sin login.
[Authorize(Policy = PermissionCodes.DriverTracking)]
public class DriverTrackingHub : Hub
{
}

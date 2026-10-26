using Microsoft.EntityFrameworkCore;

public static class GetWorkGuidesMainEndpoint{
    public static void MapGetWorkGuidesMain(this IEndpointRouteBuilder app){
        app.MapGet("/", async (RecepcionDbContext db) =>{

            var guias = await db.WorkGuideMains.Select(x => new ResponseWguidesmDto(
                x.SerieGuia,
                x.NumeroGuia,
                x.FechaOperacion,
                x.FechaHoraEntrega,
                x.MensajeAlertas,
                x.Observaciones,
                x.TipoPago,
                x.DescripcionPago,
                x.TipoRecepcion,
                x.DireccionContacto,
                x.TelefonoContacto,
                x.Total,
                x.Acuenta,
                x.Saldo,
                x.CustomerId
            )).ToListAsync();

            return Results.Ok(guias);
        }).RequirePermission(PermissionCodes.ReceptionConsultas);
    }
}
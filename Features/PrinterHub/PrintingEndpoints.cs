namespace _24dex_backend_comercial.Features.PrinterHub;

// A diferencia de PrintingAdminEndpoints (permiso Settings, pensado para el admin), este grupo
// usa el mismo permiso que ya tiene quien emite guias: cualquiera que pueda imprimir una guia
// real puede ver si la impresora esta conectada y mandar una prueba corta.
public static class PrintingEndpoints
{
    public static void MapPrinting(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/printing").WithTags("Printing").RequirePermission(PermissionCodes.ReceptionEmision);

        group.MapGetPrinterConnectionStatus();
        group.MapTestPrint();
    }
}

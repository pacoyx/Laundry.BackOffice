public static class ManagementDashboardEndpoints
{
    public static void MapManagementDashboard(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/management-dashboard")
            .WithTags("ManagementDashboard")
            .RequirePermission(PermissionCodes.Dashboard);

        group.MapGetManagementDashboard();
    }

    // E/T/D/QR/SP son los codigos ya usados en WorkGuideMain/CashBoxDetail.TipoPago.
    internal static string ResolvePaymentMethodLabel(string code) => code switch
    {
        "E" => "Efectivo",
        "T" => "Tarjeta",
        "D" => "Depósito",
        "QR" => "Código QR",
        "SP" => "Sin Pago",
        _ => string.IsNullOrWhiteSpace(code) ? "Sin especificar" : code
    };
}

using Hangfire.Dashboard;

// Restringe /hangfire a usuarios autenticados con el permiso administrativo "Settings",
// el mismo usado por Roles/Permissions. Nota: al ser JWT Bearer (no cookies), navegar
// directamente a /hangfire desde el navegador no envia el token automaticamente; esto
// protege la ruta (no queda publica) pero la visualizacion interactiva via navegador
// requiere adjuntar el Authorization header por fuera (ej. extension de browser), algo
// fuera de alcance de esta fase.
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.HasClaim("permissions", PermissionCodes.Settings);
    }
}

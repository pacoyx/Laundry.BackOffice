public static class EmployeeDocumentEndpoints
{
    public static RouteGroupBuilder MapEmployeeDocument(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("HR - Documents").RequirePermission(PermissionCodes.Employees);

        group.MapCreateEmployeeDocument();
        group.MapGetEmployeeDocuments();
        group.MapDeleteEmployeeDocument();
        group.MapDownloadEmployeeDocument();

        app.MapGetEmployeeDocumentTypes();

        return group;
    }
}

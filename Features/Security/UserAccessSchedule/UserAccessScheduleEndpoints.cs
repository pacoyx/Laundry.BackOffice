public static class UserAccessScheduleEndpoints
{
    public static void MapUserAccessSchedule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("UserAccessSchedule").RequirePermission(PermissionCodes.Settings);

        group.MapGetUserAccessSchedule();
        group.MapUpdateUserAccessSchedule();
    }
}

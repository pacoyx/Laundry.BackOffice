public static class PayrollPeriodEndpoints
{
    public static RouteGroupBuilder MapPayrollPeriod(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payroll-periods").WithTags("HR - Payroll").RequirePermission(PermissionCodes.Payroll);

        group.MapCreatePayrollPeriod();
        group.MapGetPayrollPeriods();
        group.MapDeletePayrollPeriod();

        return group;
    }
}

public static class PayrollEndpoints
{
    public static void MapPayroll(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payroll-periods").WithTags("HR - Payroll").RequirePermission(PermissionCodes.Payroll);

        group.MapGeneratePayroll();
        group.MapGetPayroll();
        group.MapDeletePayroll();
        group.MapPayrollDiscounts();
        group.MapPayrollExport();
    }
}

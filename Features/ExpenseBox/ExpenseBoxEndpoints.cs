using Microsoft.AspNetCore.Routing;

public static class ExpenseBoxEndpoints
{
    public static void MapExpenseBox(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/expenseBox").WithTags("ExpenseBox").RequirePermission(PermissionCodes.ReceptionGastos);
        group.MapCreateExpenseBox();
        group.MapGetExpenseBox();
        group.MapGetExpensesBox();
        group.MapGetExpensesBoxByDate();
        group.MapDeleteExpenseBox();
        group.MapUpdateExpenseBox();
    }
}
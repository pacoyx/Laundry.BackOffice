public static class CustomerPermissionCodes
{
    public const string GuidesView = "guides.view";
    public const string InvoicesView = "invoices.view";
    public const string ReportsView = "reports.view";
    public const string UsersManage = "users.manage";
    public const string GarmentsManage = "garments.manage";

    public static readonly string[] All =
    {
        GuidesView, InvoicesView, ReportsView, UsersManage, GarmentsManage
    };
}

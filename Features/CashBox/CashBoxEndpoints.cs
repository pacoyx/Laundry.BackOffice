public static class CashBoxEndpoints{
    public static void MapCashBox(this IEndpointRouteBuilder app){
        var group = app.MapGroup("/api/cashBox").RequirePermission(PermissionCodes.ReceptionCaja);
        group.MapCreateCashBox();
        group.MapCreateCashBoxDetail();
        group.MapGetCashBox();
        group.MapGetCashBoxes();
        group.MapDeleteCashBox();
        group.MapGetCashBoxOpenByUser();
        group.MapCloseCashBox();
        group.MapGetItemsCashBoxDetail();
        group.MapCreateCashBoxDetailOtros();
        group.MapDeleteCashBoxDetail();
        group.MapGetCashBoxResume();
        group.MapGetCashBoxDetailByIdAndTp();
        group.MapGetCashBoxResumeAllUser();
        group.MapSplitPayCash();
    }
}
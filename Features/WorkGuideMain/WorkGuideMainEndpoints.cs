public static class WorkGuideMainEndpoints{
    public static void MapWorkGuideMain(this IEndpointRouteBuilder app){
        var group = app.MapGroup("/api/workGuideMain");
        group.MapCreateWorkGuideMain();
        group.MapGetWorkGuideMain();
        group.MapGetWorkGuidesMain();
        group.MapDeleteWorkGuideMain();        
        group.MapGetWorkGuideByDocument();
        group.MapUpdateWorkGuideInfoPay();
        group.MapUpdateWorkGuidePickupDate();
        group.MapCancelWorkGuide();
        group.MapCreateAlertWorkGuide();
        group.MapGetAlertsByWorkGuide();
        group.MapCancelItemWorkGuide();
        group.MapAddWorkGuideItem();
        group.MapAdjustWorkGuideItemQuantity();
        group.MapReturnUnwashedClothes();
        group.MapPickUpItem();
        group.MapReprintWorkGuideMain();
    }
}
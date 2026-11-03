// Exactamente uno de los dos debe venir con valor: NewQuantity (ajusta cantidad, el precio
// unitario no cambia) o NewTotal (ajusta el total; si el servicio es por peso se recalcula la
// cantidad manteniendo el precio unitario, si es por unidad se recalcula el precio unitario
// manteniendo la cantidad -ver AdjustWorkGuideItemQuantityEndpoint).
public record AdjustWorkGuideItemQuantityRequest(decimal? NewQuantity, decimal? NewTotal, string? Observation);

public record AdjustWorkGuideItemQuantityResponse(int WorkGuideDetailId, decimal NewQuantity, decimal NewPrecio, decimal NewItemTotal, decimal NewTotal);

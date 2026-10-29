public record AdjustWorkGuideItemQuantityRequest(decimal NewQuantity, string? Observation);

public record AdjustWorkGuideItemQuantityResponse(int WorkGuideDetailId, decimal NewTotal);

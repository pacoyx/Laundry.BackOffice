public record AddWorkGuideItemRequest(int ServiceItemId, decimal Quantity, decimal UnitPrice, string? Observation);

public record AddWorkGuideItemResponse(int WorkGuideDetailId, decimal NewTotal);

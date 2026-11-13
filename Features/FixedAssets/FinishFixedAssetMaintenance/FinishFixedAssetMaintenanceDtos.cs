public record FinishFixedAssetMaintenanceRequest(
    DateTime ActualEndDate,
    decimal? FinalCost,
    string? ClosingNotes);

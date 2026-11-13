/// <summary>
/// Filtros del reporte de historial. Las fechas filtran los EVENTOS (y los costos/días del resumen);
/// sucursal, categoría, estado y activo filtran los ACTIVOS incluidos.
/// </summary>
public record FixedAssetHistoryReportFilter(
    DateTime? FromDate,
    DateTime? ToDate,
    int? BranchId,
    int? CategoryId,
    FixedAssetStatus? Status,
    long? FixedAssetId);

public static class FixedAssetHistoryEventTypes
{
    public const string Acquisition = "acquisition";
    public const string Reassignment = "reassignment";
    public const string Maintenance = "maintenance";
    public const string OutOfService = "out-of-service";
    public const string Disposal = "disposal";
}

public record FixedAssetHistoryEventDto(
    DateTime Date,
    long FixedAssetId,
    string AssetCode,
    string AssetName,
    string CategoryName,
    string BranchName,
    string EventType,
    string EventLabel,
    string Description,
    string? Detail,
    decimal? Cost,
    DateTime? EndDate,
    int? DaysOutOfService,
    bool InProgress);

public record FixedAssetHistorySummaryDto(
    long FixedAssetId,
    string Code,
    string Name,
    string CategoryName,
    string BranchName,
    string? ResponsibleUserName,
    FixedAssetStatus Status,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    decimal BookValue,
    int MaintenanceCount,
    decimal MaintenanceCost,
    int OutOfServiceCount,
    int DaysOutOfService,
    int ReassignmentCount,
    DateTime? LastMaintenanceDate,
    DateTime? NextMaintenanceDate);

public record FixedAssetHistoryTotalsDto(
    int AssetCount,
    decimal TotalAcquisitionCost,
    decimal TotalBookValue,
    int MaintenanceCount,
    decimal TotalMaintenanceCost,
    int TotalDaysOutOfService,
    int AssetsCurrentlyInMaintenance);

public record FixedAssetHistoryReportResponse(
    List<FixedAssetHistorySummaryDto> Assets,
    List<FixedAssetHistoryEventDto> Events,
    FixedAssetHistoryTotalsDto Totals);

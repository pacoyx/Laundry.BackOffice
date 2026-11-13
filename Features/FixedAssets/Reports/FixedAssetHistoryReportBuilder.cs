using Microsoft.EntityFrameworkCore;

/// <summary>
/// Arma el historial de activos fijos (eventos + resumen por activo) a partir de las tablas
/// FixedAssets, FixedAssetMaintenances y FixedAssetReassignments. Lo usan la pantalla de
/// reporte, el Excel y la ficha PDF, para que los tres muestren exactamente lo mismo.
/// </summary>
public static class FixedAssetHistoryReportBuilder
{
    public static async Task<List<FixedAsset>> LoadAssetsAsync(
        RecepcionDbContext db,
        FixedAssetHistoryReportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = db.FixedAssets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Category)
            .Include(x => x.Branch)
            .Include(x => x.ResponsibleUser)
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseInvoice)
            .Include(x => x.Maintenances)
            .Include(x => x.Reassignments).ThenInclude(r => r.PreviousBranch)
            .Include(x => x.Reassignments).ThenInclude(r => r.NewBranch)
            .Include(x => x.Reassignments).ThenInclude(r => r.NewResponsibleUser)
            .AsQueryable();

        if (filter.FixedAssetId.HasValue) query = query.Where(x => x.Id == filter.FixedAssetId.Value);
        if (filter.BranchId.HasValue) query = query.Where(x => x.BranchId == filter.BranchId.Value);
        if (filter.CategoryId.HasValue) query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        if (filter.Status.HasValue) query = query.Where(x => x.Status == filter.Status.Value);

        return await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
    }

    public static FixedAssetHistoryReportResponse Build(List<FixedAsset> assets, FixedAssetHistoryReportFilter filter)
    {
        var from = filter.FromDate?.Date;
        // "Hasta" incluye todo el día
        var toExclusive = filter.ToDate?.Date.AddDays(1);
        var today = DateTime.Today;

        bool InRange(DateTime date) =>
            (from is null || date >= from) && (toExclusive is null || date < toExclusive);

        var events = new List<FixedAssetHistoryEventDto>();
        var summaries = new List<FixedAssetHistorySummaryDto>();

        foreach (var asset in assets)
        {
            FixedAssetHistoryEventDto NewEvent(
                DateTime date, string type, string label, string description, string? detail = null,
                decimal? cost = null, DateTime? endDate = null, int? days = null, bool inProgress = false) =>
                new(date, asset.Id, asset.Code, asset.Name, asset.Category.Name, asset.Branch.Description,
                    type, label, description, detail, cost, endDate, days, inProgress);

            // Adquisición
            if (InRange(asset.AcquisitionDate))
            {
                events.Add(NewEvent(asset.AcquisitionDate, FixedAssetHistoryEventTypes.Acquisition, "Adquisición",
                    "Alta del activo",
                    asset.Supplier is null ? null : $"Proveedor: {asset.Supplier.Name}",
                    asset.AcquisitionCost));
            }

            // Reasignaciones
            var reassignments = asset.Reassignments.Where(r => InRange(r.Date)).ToList();
            foreach (var r in reassignments)
            {
                var detail = string.Join(" · ", new[]
                {
                    r.PreviousBranch is null ? null : $"De: {r.PreviousBranch.Description}",
                    r.NewResponsibleUser is null ? null : $"Responsable: {r.NewResponsibleUser.Name}",
                    string.IsNullOrWhiteSpace(r.Reason) ? null : $"Motivo: {r.Reason}",
                }.Where(s => s is not null));

                events.Add(NewEvent(r.Date, FixedAssetHistoryEventTypes.Reassignment, "Reasignación",
                    $"Reasignado a {r.NewBranch.Description}", string.IsNullOrEmpty(detail) ? null : detail));
            }

            // Mantenimientos (y paradas "En Mantenimiento")
            var maintenanceCount = 0;
            var maintenanceCost = 0m;
            var outOfServiceCount = 0;
            var daysOutOfService = 0;

            foreach (var m in asset.Maintenances.OrderBy(m => m.Date))
            {
                var typeLabel = m.Type == FixedAssetMaintenanceType.Preventive ? "Preventivo" : "Correctivo";

                if (!m.PutsAssetOutOfService)
                {
                    if (!InRange(m.Date)) continue;

                    maintenanceCount++;
                    maintenanceCost += m.Cost;
                    events.Add(NewEvent(m.Date, FixedAssetHistoryEventTypes.Maintenance, $"Mantenimiento {typeLabel.ToLower()}",
                        m.Description, JoinDetail(m.ServiceProvider, m.Observations), m.Cost));
                    continue;
                }

                // Parada: cuenta si el periodo fuera de servicio se cruza con el rango del filtro
                var start = m.Date.Date;
                var inProgress = m.ActualEndDate is null;
                var end = (m.ActualEndDate ?? today).Date;
                var overlapStart = from is null || start > from ? start : from.Value;
                var overlapEnd = toExclusive is null || end < toExclusive ? end : toExclusive.Value.AddDays(-1);
                if (overlapEnd < overlapStart) continue;

                var days = Math.Max(1, (overlapEnd - overlapStart).Days);

                maintenanceCount++;
                maintenanceCost += m.Cost;
                outOfServiceCount++;
                daysOutOfService += days;

                var periodo = inProgress
                    ? $"En curso desde {start:dd/MM/yyyy} (fin estimado {m.EstimatedEndDate:dd/MM/yyyy})"
                    : $"Fuera de servicio del {start:dd/MM/yyyy} al {end:dd/MM/yyyy}";
                if (!inProgress && m.EstimatedEndDate.HasValue && end > m.EstimatedEndDate.Value.Date)
                {
                    periodo += $" — {(end - m.EstimatedEndDate.Value.Date).Days} día(s) de retraso";
                }

                events.Add(NewEvent(m.Date, FixedAssetHistoryEventTypes.OutOfService, $"En mantenimiento ({typeLabel.ToLower()})",
                    m.Description,
                    JoinDetail($"Motivo: {m.OutOfServiceReason}", periodo, m.ServiceProvider, m.Observations,
                        string.IsNullOrWhiteSpace(m.ClosingNotes) ? null : $"Cierre: {m.ClosingNotes}"),
                    m.Cost, m.ActualEndDate, days, inProgress));
            }

            // Baja / venta
            if (asset.DisposalDate.HasValue && InRange(asset.DisposalDate.Value))
            {
                events.Add(NewEvent(asset.DisposalDate.Value, FixedAssetHistoryEventTypes.Disposal,
                    asset.Status == FixedAssetStatus.Sold ? "Venta" : "Baja",
                    asset.Status == FixedAssetStatus.Sold ? "Activo vendido" : "Activo dado de baja",
                    asset.DisposalReason));
            }

            var lastMaintenance = asset.Maintenances.OrderByDescending(m => m.Date).FirstOrDefault();

            summaries.Add(new FixedAssetHistorySummaryDto(
                asset.Id,
                asset.Code,
                asset.Name,
                asset.Category.Name,
                asset.Branch.Description,
                asset.ResponsibleUser?.Name,
                asset.Status,
                asset.AcquisitionDate,
                asset.AcquisitionCost,
                FixedAssetDepreciationCalculator.GetBookValue(asset),
                maintenanceCount,
                maintenanceCost,
                outOfServiceCount,
                daysOutOfService,
                reassignments.Count,
                lastMaintenance?.Date,
                lastMaintenance?.NextMaintenanceDate));
        }

        var orderedEvents = events
            .OrderByDescending(e => e.Date)
            .ThenBy(e => e.AssetCode)
            .ToList();

        var totals = new FixedAssetHistoryTotalsDto(
            summaries.Count,
            summaries.Sum(s => s.AcquisitionCost),
            summaries.Sum(s => s.BookValue),
            summaries.Sum(s => s.MaintenanceCount),
            summaries.Sum(s => s.MaintenanceCost),
            summaries.Sum(s => s.DaysOutOfService),
            summaries.Count(s => s.Status == FixedAssetStatus.InMaintenance));

        return new FixedAssetHistoryReportResponse(summaries, orderedEvents, totals);
    }

    private static string? JoinDetail(params string?[] parts)
    {
        var text = string.Join(" · ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        return text.Length == 0 ? null : text;
    }

    public static string StatusLabel(FixedAssetStatus status) => status switch
    {
        FixedAssetStatus.Active => "Activo",
        FixedAssetStatus.InMaintenance => "En Mantenimiento",
        FixedAssetStatus.Disposed => "De baja",
        FixedAssetStatus.Sold => "Vendido",
        _ => status.ToString()
    };
}

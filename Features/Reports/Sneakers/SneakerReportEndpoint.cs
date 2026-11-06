using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class SneakerReportEndpoint
{
    public static void MapSneakerReport(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sneakers", async (
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int? branchId,
            [FromQuery] int? brandId,
            [FromQuery] int? customerId,
            [FromQuery] string? ticketStatus,
            [FromQuery] bool? pendingDelivery,
            IValidator<SneakerReportFilter> validator,
            RecepcionDbContext db) =>
        {
            var filter = new SneakerReportFilter
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                BranchId = branchId,
                BrandId = brandId,
                CustomerId = customerId,
                TicketStatus = ticketStatus,
                PendingDelivery = pendingDelivery
            };

            var validation = await validator.ValidateAsync(filter);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var query = db.SneakerDetails.AsNoTracking().AsQueryable();

            if (filter.DateFrom.HasValue)
                query = query.Where(s => s.WorkGuideDetail.WorkGuideMain!.FechaOperacion.Date >= filter.DateFrom.Value.Date);

            if (filter.DateTo.HasValue)
                query = query.Where(s => s.WorkGuideDetail.WorkGuideMain!.FechaOperacion.Date <= filter.DateTo.Value.Date);

            if (filter.BranchId.HasValue)
                query = query.Where(s => s.WorkGuideDetail.WorkGuideMain!.BranchId == filter.BranchId.Value);

            if (filter.BrandId.HasValue)
                query = query.Where(s => s.SneakerBrandId == filter.BrandId.Value);

            if (filter.CustomerId.HasValue)
                query = query.Where(s => s.WorkGuideDetail.WorkGuideMain!.CustomerId == filter.CustomerId.Value);

            if (!string.IsNullOrEmpty(filter.TicketStatus))
                query = query.Where(s => s.WorkGuideDetail.EstadoTrabajo == filter.TicketStatus);

            if (filter.PendingDelivery == true)
                query = query.Where(s => s.WorkGuideDetail.EstadoSituacion == "P");

            var rows = await query
                .OrderByDescending(s => s.WorkGuideDetail.WorkGuideMain!.FechaOperacion)
                .ThenByDescending(s => s.Id)
                .Select(s => new
                {
                    WorkGuideId = s.WorkGuideDetail.WorkGuideMain!.Id,
                    WorkGuideNumber = s.WorkGuideDetail.WorkGuideMain!.SerieGuia + "-" + s.WorkGuideDetail.WorkGuideMain.NumeroGuia,
                    WorkGuideDate = s.WorkGuideDetail.WorkGuideMain.FechaOperacion,
                    CustomerName = (s.WorkGuideDetail.WorkGuideMain.Customer!.FirtsName + " " + (s.WorkGuideDetail.WorkGuideMain.Customer.LastName ?? "")).Trim(),
                    SneakerDetailId = s.Id,
                    Brand = s.Brand != null ? s.Brand.Name : null,
                    s.PrimaryColor,
                    s.HasLaces,
                    s.HasInsoles,
                    DeliveryStatusCode = s.WorkGuideDetail.EstadoSituacion,
                    PhotoCount = db.ItemPhotos.Count(p => p.WorkGuideMainId == s.WorkGuideDetail.WorkGuideMainId)
                })
                .ToListAsync();

            // Se agrupa en memoria (no en SQL) porque GroupBy conserva el orden de primera
            // aparicion de cada clave, que ya viene ordenado desc por fecha desde la consulta
            // de arriba -asi no hay que volver a ordenar los grupos.
            var response = rows
                .GroupBy(r => new { r.WorkGuideId, r.WorkGuideNumber, r.WorkGuideDate, r.CustomerName, r.PhotoCount })
                .Select(g => new SneakerReportGroupResponse(
                    g.Key.WorkGuideId,
                    g.Key.WorkGuideNumber,
                    g.Key.WorkGuideDate,
                    g.Key.CustomerName,
                    g.Count(),
                    g.All(x => x.DeliveryStatusCode == "E") ? "Delivered" : "Pending",
                    g.Key.PhotoCount,
                    g.Select(x => new SneakerReportItemResponse(
                        x.SneakerDetailId,
                        x.Brand,
                        x.PrimaryColor,
                        x.HasLaces,
                        x.HasInsoles
                    )).ToList()
                ))
                .ToList();

            return Results.Ok(new ApiResponse<List<SneakerReportGroupResponse>>
            {
                Success = true,
                Message = "Reporte de zapatillas.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }
}

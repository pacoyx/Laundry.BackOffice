using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreatePickupGuideRevisionEndpoint
{
    public static void MapCreatePickupGuideRevision(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/revisions", async (
            long id,
            CreatePickupGuideRevisionRequest request,
            IValidator<CreatePickupGuideRevisionRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var guide = await db.PickupGuides
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (guide is null)
            {
                return Results.NotFound("Guía no encontrada.");
            }

            // La revisión de planta corrige cantidades mientras la guía sigue en Draft;
            // una vez Collected, las cantidades quedan congeladas en la constancia emitida.
            if (guide.Status != PickupGuideStatus.Draft)
            {
                return Results.BadRequest(new[] { $"Solo se pueden registrar revisiones sobre guías en estado 'Draft'. Estado actual: '{guide.Status}'." });
            }

            var employeeExists = await db.Employees.AnyAsync(x => x.Id == request.EmployeeId && x.IsActive);
            if (!employeeExists)
            {
                return Results.BadRequest(new[] { "Empleado no encontrado o inactivo." });
            }

            var existingItemDetailIds = request.Details
                .Where(d => d.ActionType is PickupGuideRevisionActionType.QuantityAdjustment or PickupGuideRevisionActionType.RemovedItem)
                .Select(d => d.PickupGuideDetailId!.Value)
                .ToList();
            var missingDetailIds = existingItemDetailIds.Except(guide.Details.Select(d => d.Id)).ToList();
            if (missingDetailIds.Count > 0)
            {
                return Results.BadRequest(new[] { $"Los siguientes detalles no pertenecen a esta guía: {string.Join(", ", missingDetailIds)}" });
            }

            var addedItemRequests = request.Details
                .Where(d => d.ActionType == PickupGuideRevisionActionType.AddedItem)
                .ToList();

            var priceDetailsByServiceItemId = new Dictionary<int, CustomerPriceListDetail>();
            if (addedItemRequests.Count > 0)
            {
                var serviceItemIds = addedItemRequests.Select(d => d.ServiceItemId!.Value).Distinct().ToList();

                var serviceItems = await db.ProdServices
                    .Where(s => serviceItemIds.Contains(s.Id) && s.Status == "A")
                    .ToListAsync();
                var missingServiceItemIds = serviceItemIds.Except(serviceItems.Select(s => s.Id)).ToList();
                if (missingServiceItemIds.Count > 0)
                {
                    return Results.BadRequest(new[] { $"Los siguientes servicios no existen o están inactivos: {string.Join(", ", missingServiceItemIds)}" });
                }

                var priceDetails = await db.CustomerPriceListDetails
                    .Include(x => x.ServiceItem)
                    .Where(x => x.CustomerPriceListId == guide.CustomerPriceListId && serviceItemIds.Contains(x.ServiceItemId))
                    .ToListAsync();

                var unpricedServiceItemIds = serviceItemIds.Except(priceDetails.Select(x => x.ServiceItemId)).ToList();
                if (unpricedServiceItemIds.Count > 0)
                {
                    var unpricedNames = serviceItems.Where(s => unpricedServiceItemIds.Contains(s.Id)).Select(s => s.Name);
                    return Results.BadRequest(new[] { $"Los siguientes servicios no están tarifados para este cliente: {string.Join(", ", unpricedNames)}" });
                }

                priceDetailsByServiceItemId = priceDetails.ToDictionary(x => x.ServiceItemId);
            }

            DateTime fechaOperacion = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            await using var transaction = await db.Database.BeginTransactionAsync();

            var revision = new PickupGuideRevision
            {
                PickupGuideId = id,
                EmployeeId = request.EmployeeId,
                Observation = request.Observation ?? string.Empty,
                RevisionDate = fechaOperacion
            };

            foreach (var detailRequest in request.Details)
            {
                PickupGuideDetail detail;
                decimal originalQuantity;
                decimal newQuantity;

                switch (detailRequest.ActionType)
                {
                    case PickupGuideRevisionActionType.QuantityAdjustment:
                    {
                        detail = guide.Details.First(d => d.Id == detailRequest.PickupGuideDetailId!.Value);
                        var isPricedByWeight = detail.ServiceItem.IsPeso == "S";
                        originalQuantity = isPricedByWeight ? detail.Weight ?? 0 : detail.Quantity;
                        newQuantity = detailRequest.NewQuantity!.Value;

                        if (isPricedByWeight)
                        {
                            detail.Weight = newQuantity;
                        }
                        else
                        {
                            detail.Quantity = newQuantity;
                        }

                        detail.Total = newQuantity * detail.UnitPrice;
                        break;
                    }

                    case PickupGuideRevisionActionType.AddedItem:
                    {
                        var priceDetail = priceDetailsByServiceItemId[detailRequest.ServiceItemId!.Value];
                        var isPricedByWeight = priceDetail.ServiceItem.IsPeso == "S";
                        newQuantity = detailRequest.NewQuantity!.Value;
                        originalQuantity = 0;

                        detail = new PickupGuideDetail
                        {
                            PickupGuideId = id,
                            ServiceItemId = detailRequest.ServiceItemId!.Value,
                            ServiceItem = priceDetail.ServiceItem,
                            Quantity = isPricedByWeight ? 0 : newQuantity,
                            Weight = isPricedByWeight ? newQuantity : null,
                            UnitPrice = priceDetail.UnitPrice,
                            Total = newQuantity * priceDetail.UnitPrice
                        };
                        guide.Details.Add(detail);
                        break;
                    }

                    case PickupGuideRevisionActionType.RemovedItem:
                    {
                        detail = guide.Details.First(d => d.Id == detailRequest.PickupGuideDetailId!.Value);
                        var isPricedByWeight = detail.ServiceItem.IsPeso == "S";
                        originalQuantity = isPricedByWeight ? detail.Weight ?? 0 : detail.Quantity;
                        newQuantity = 0;

                        if (isPricedByWeight)
                        {
                            detail.Weight = 0;
                        }
                        else
                        {
                            detail.Quantity = 0;
                        }

                        detail.Total = 0;
                        break;
                    }

                    default:
                        throw new InvalidOperationException($"Tipo de acción de revisión no soportado: {detailRequest.ActionType}");
                }

                revision.DetailRevisions.Add(new PickupGuideDetailRevision
                {
                    PickupGuideDetail = detail,
                    ActionType = detailRequest.ActionType,
                    OriginalQuantity = originalQuantity,
                    NewQuantity = newQuantity,
                    Reason = detailRequest.Reason
                });
            }

            guide.TotalAmount = guide.Details.Sum(d => d.Total);
            guide.UpdatedAt = fechaOperacion;

            await db.PickupGuideRevisions.AddAsync(revision);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            await db.Entry(revision).Collection(x => x.DetailRevisions).Query()
                .Include(x => x.PickupGuideDetail).ThenInclude(d => d.ServiceItem)
                .LoadAsync();
            await db.Entry(revision).Reference(x => x.Employee).LoadAsync();

            var response = new ApiResponse<PickupGuideRevisionResponse>
            {
                Success = true,
                Message = "Revisión registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = PickupGuideRevisionsEndpoints.ToResponse(revision)
            };

            return Results.Created($"/api/pickup-guides/{id}/revisions", response);
        })
        .WithName("CreatePickupGuideRevision")
        .Produces<ApiResponse<PickupGuideRevisionResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

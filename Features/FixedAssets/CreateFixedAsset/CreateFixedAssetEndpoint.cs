using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateFixedAssetEndpoint
{
    public static void MapCreateFixedAsset(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateFixedAssetRequest request,
            IValidator<CreateFixedAssetRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var categoryExists = await db.FixedAssetCategories.AnyAsync(x => x.Id == request.CategoryId);
            if (!categoryExists)
            {
                return Results.BadRequest(new[] { "La categoría indicada no existe." });
            }

            var branchExists = await db.BranchSales.AnyAsync(x => x.Id == request.BranchId);
            if (!branchExists)
            {
                return Results.BadRequest(new[] { "La sucursal indicada no existe." });
            }

            if (request.SupplierId is not null && !await db.Suppliers.AnyAsync(x => x.Id == request.SupplierId))
            {
                return Results.BadRequest(new[] { "El proveedor indicado no existe." });
            }

            if (request.PurchaseInvoiceId is not null && !await db.PurchaseInvoices.AnyAsync(x => x.Id == request.PurchaseInvoiceId))
            {
                return Results.BadRequest(new[] { "La factura de compra indicada no existe." });
            }

            if (request.ResponsibleUserId is not null && !await db.Users.AnyAsync(x => x.Id == request.ResponsibleUserId))
            {
                return Results.BadRequest(new[] { "El usuario responsable indicado no existe." });
            }

            var code = await GenerateNextCodeAsync(db);

            var asset = new FixedAsset
            {
                Code = code,
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                AcquisitionDate = request.AcquisitionDate,
                AcquisitionCost = request.AcquisitionCost,
                SupplierId = request.SupplierId,
                PurchaseInvoiceId = request.PurchaseInvoiceId,
                BranchId = request.BranchId,
                ResponsibleUserId = request.ResponsibleUserId,
                UsefulLifeYearsOverride = request.UsefulLifeYearsOverride,
                Status = FixedAssetStatus.Active,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await db.FixedAssets.AddAsync(asset);
            await db.SaveChangesAsync();

            var saved = await db.FixedAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Branch)
                .Include(x => x.ResponsibleUser)
                .FirstAsync(x => x.Id == asset.Id);

            var response = new ApiResponse<FixedAssetListItemResponse>
            {
                Success = true,
                Message = "Activo fijo registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = FixedAssetsEndpoints.ToListItemResponse(saved)
            };

            return Results.Created($"/api/fixed-assets/{asset.Id}", response);
        })
        .WithName("CreateFixedAsset")
        .Produces<ApiResponse<FixedAssetListItemResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<string> GenerateNextCodeAsync(RecepcionDbContext db)
    {
        var lastCode = await db.FixedAssets
            .OrderByDescending(x => x.Id)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("AF-") && int.TryParse(lastCode[3..], out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"AF-{nextNumber:D4}";
    }
}

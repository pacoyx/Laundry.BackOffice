using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateFixedAssetEndpoint
{
    public static void MapUpdateFixedAsset(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:long}", async (
            long id,
            UpdateFixedAssetRequest request,
            IValidator<UpdateFixedAssetRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            var categoryExists = await db.FixedAssetCategories.AnyAsync(x => x.Id == request.CategoryId);
            if (!categoryExists)
            {
                return Results.BadRequest(new[] { "La categoría indicada no existe." });
            }

            if (request.SupplierId is not null && !await db.Suppliers.AnyAsync(x => x.Id == request.SupplierId))
            {
                return Results.BadRequest(new[] { "El proveedor indicado no existe." });
            }

            if (request.PurchaseInvoiceId is not null && !await db.PurchaseInvoices.AnyAsync(x => x.Id == request.PurchaseInvoiceId))
            {
                return Results.BadRequest(new[] { "La factura de compra indicada no existe." });
            }

            asset.Name = request.Name;
            asset.Description = request.Description;
            asset.CategoryId = request.CategoryId;
            asset.AcquisitionDate = request.AcquisitionDate;
            asset.AcquisitionCost = request.AcquisitionCost;
            asset.SupplierId = request.SupplierId;
            asset.PurchaseInvoiceId = request.PurchaseInvoiceId;
            asset.UsefulLifeYearsOverride = request.UsefulLifeYearsOverride;
            asset.Notes = request.Notes;

            await db.SaveChangesAsync();

            var saved = await db.FixedAssets
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Branch)
                .Include(x => x.ResponsibleUser)
                .FirstAsync(x => x.Id == id);

            return Results.Ok(new ApiResponse<FixedAssetListItemResponse>
            {
                Success = true,
                Message = "Activo fijo actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = FixedAssetsEndpoints.ToListItemResponse(saved)
            });
        })
        .WithName("UpdateFixedAsset")
        .Produces<ApiResponse<FixedAssetListItemResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

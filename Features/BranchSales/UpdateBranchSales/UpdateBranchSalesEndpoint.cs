using Microsoft.EntityFrameworkCore;

public static class UpdateBranchSalesEndpoint
{
    public static void MapUpdateBranchSales(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (RecepcionDbContext context, int id, UpdateBranchSalesDto request) =>
        {
            var branchSale = await context.BranchSales
            .FirstOrDefaultAsync(bs => bs.Id == id);

            if (branchSale == null)
            {
                return Results.NotFound();
            }

            branchSale.Description = request.Description;
            branchSale.Address = request.Address;
            branchSale.Status = request.Status;
            branchSale.Code = request.Code;
            branchSale.DefaultPrinterId = request.DefaultPrinterId;

            if (request.IsCentral && !branchSale.IsCentral)
            {
                var currentCentral = await context.BranchSales
                    .Where(x => x.IsCentral && x.Id != id)
                    .ToListAsync();
                foreach (var other in currentCentral)
                {
                    other.IsCentral = false;
                }
            }

            branchSale.IsCentral = request.IsCentral;

            await context.SaveChangesAsync();

            var response = new ApiResponse<string>
            {
                Data = "Sucursal actualizada",
                Message = "Sucursal actualizada",
                StatusCode = StatusCodes.Status200OK,
                Success = true
            };

            return Results.Ok(response);
        });
    }
}

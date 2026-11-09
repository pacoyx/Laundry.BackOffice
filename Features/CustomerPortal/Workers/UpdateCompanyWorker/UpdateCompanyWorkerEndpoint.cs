using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateCompanyWorkerEndpoint
{
    public static void MapUpdateCompanyWorker(this IEndpointRouteBuilder app)
    {
        app.MapPut("/workers/{id:int}", async (
            int id,
            ClaimsPrincipal principal,
            UpdateCompanyWorkerRequest request,
            IValidator<UpdateCompanyWorkerRequest> validator,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var worker = await db.CompanyWorkers
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (worker is null)
            {
                return Results.NotFound();
            }

            var codeTaken = await db.CompanyWorkers
                .AnyAsync(x => x.CustomerId == customerId.Value && x.Code == request.Code && x.Id != id);
            if (codeTaken)
            {
                return Results.BadRequest(new[] { "Ya existe un trabajador con ese código." });
            }

            worker.Code = request.Code;
            worker.FullName = request.FullName;
            worker.Area = request.Area;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<CompanyWorkerResponse>
            {
                Success = true,
                Message = "Trabajador actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = CompanyWorkersEndpoints.ToResponse(worker)
            });
        })
        .WithName("UpdateCompanyWorker")
        .Produces<ApiResponse<CompanyWorkerResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

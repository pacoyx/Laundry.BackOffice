using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateCompanyWorkerEndpoint
{
    public static void MapCreateCompanyWorker(this IEndpointRouteBuilder app)
    {
        app.MapPost("/workers", async (
            ClaimsPrincipal principal,
            CreateCompanyWorkerRequest request,
            IValidator<CreateCompanyWorkerRequest> validator,
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

            var codeTaken = await db.CompanyWorkers
                .AnyAsync(x => x.CustomerId == customerId.Value && x.Code == request.Code);
            if (codeTaken)
            {
                return Results.BadRequest(new[] { "Ya existe un trabajador con ese código." });
            }

            var worker = new CompanyWorker
            {
                CustomerId = customerId.Value,
                Code = request.Code,
                FullName = request.FullName,
                Area = request.Area,
                IsActive = true
            };

            await db.CompanyWorkers.AddAsync(worker);
            await db.SaveChangesAsync();

            var response = new ApiResponse<CompanyWorkerResponse>
            {
                Success = true,
                Message = "Trabajador registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = CompanyWorkersEndpoints.ToResponse(worker)
            };

            return Results.Created($"/api/customer-portal/workers/{worker.Id}", response);
        })
        .WithName("CreateCompanyWorker")
        .Produces<ApiResponse<CompanyWorkerResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

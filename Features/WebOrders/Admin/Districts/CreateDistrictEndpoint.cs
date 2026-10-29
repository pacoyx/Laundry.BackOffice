using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateDistrictEndpoint
{
    public static void MapCreateDistrict(this IEndpointRouteBuilder app)
    {
        app.MapPost("/districts", async (
            CreateDistrictRequest request,
            IValidator<CreateDistrictRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var zone = await db.Zones.FirstOrDefaultAsync(x => x.Id == request.ZoneId);
            if (zone is null)
            {
                return Results.BadRequest(new[] { "La zona indicada no existe." });
            }

            var nameExists = await db.Districts.AnyAsync(x => x.Name == request.Name);
            if (nameExists)
            {
                return Results.BadRequest(new[] { "Ya existe un distrito con ese nombre." });
            }

            var district = new District { Name = request.Name, ZoneId = request.ZoneId, IsActive = true };
            db.Districts.Add(district);
            await db.SaveChangesAsync();

            return Results.Created($"/api/web-orders/admin/districts/{district.Id}", new ApiResponse<DistrictAdminResponse>
            {
                Success = true,
                Message = "Distrito creado.",
                StatusCode = StatusCodes.Status201Created,
                Data = new DistrictAdminResponse(district.Id, district.Name, district.ZoneId, zone.Name, district.IsActive)
            });
        })
        .WithName("CreateDistrict")
        .Produces<ApiResponse<DistrictAdminResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}

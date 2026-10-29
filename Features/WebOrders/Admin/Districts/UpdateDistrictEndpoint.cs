using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateDistrictEndpoint
{
    public static void MapUpdateDistrict(this IEndpointRouteBuilder app)
    {
        app.MapPut("/districts/{id:int}", async (
            int id,
            UpdateDistrictRequest request,
            IValidator<UpdateDistrictRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var district = await db.Districts.FirstOrDefaultAsync(x => x.Id == id);
            if (district is null)
            {
                return Results.NotFound();
            }

            var zone = await db.Zones.FirstOrDefaultAsync(x => x.Id == request.ZoneId);
            if (zone is null)
            {
                return Results.BadRequest(new[] { "La zona indicada no existe." });
            }

            var nameExists = await db.Districts.AnyAsync(x => x.Name == request.Name && x.Id != id);
            if (nameExists)
            {
                return Results.BadRequest(new[] { "Ya existe un distrito con ese nombre." });
            }

            district.Name = request.Name;
            district.ZoneId = request.ZoneId;
            district.IsActive = request.IsActive;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<DistrictAdminResponse>
            {
                Success = true,
                Message = "Distrito actualizado.",
                StatusCode = StatusCodes.Status200OK,
                Data = new DistrictAdminResponse(district.Id, district.Name, district.ZoneId, zone.Name, district.IsActive)
            });
        })
        .WithName("UpdateDistrict")
        .Produces<ApiResponse<DistrictAdminResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}

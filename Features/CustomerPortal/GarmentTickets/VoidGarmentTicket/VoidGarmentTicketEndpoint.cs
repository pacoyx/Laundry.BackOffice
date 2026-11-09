using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class VoidGarmentTicketEndpoint
{
    public static void MapVoidGarmentTicket(this IEndpointRouteBuilder app)
    {
        app.MapPost("/garment-tickets/{id:int}/void", async (
            int id,
            ClaimsPrincipal principal,
            VoidGarmentIntakeTicketRequest request,
            IValidator<VoidGarmentIntakeTicketRequest> validator,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            var customerUserId = principal.GetCustomerUserId();
            if (customerId is null || customerUserId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var ticket = await db.GarmentIntakeTickets
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (ticket is null)
            {
                return Results.NotFound();
            }

            if (ticket.Status != GarmentIntakeTicketStatus.Pending)
            {
                return Results.BadRequest(new[] { "Solo se pueden anular tickets pendientes." });
            }

            ticket.Status = GarmentIntakeTicketStatus.Voided;
            ticket.VoidedReason = request.Reason;
            ticket.VoidedAt = DateTime.UtcNow;
            ticket.VoidedByCustomerUserId = customerUserId.Value;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Ticket anulado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { ticket.Id, ticket.Status }
            });
        })
        .WithName("VoidGarmentTicket")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}

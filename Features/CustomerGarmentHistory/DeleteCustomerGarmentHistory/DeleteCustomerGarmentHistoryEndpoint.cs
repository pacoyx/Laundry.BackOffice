using Microsoft.EntityFrameworkCore;

public static class DeleteCustomerGarmentHistoryEndpoint
{
    public static void MapDeleteCustomerGarmentHistory(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (int id, RecepcionDbContext db) =>
        {
            var item = await db.CustomerGarmentHistories.FirstOrDefaultAsync(h => h.Id == id);
            if (item == null)
            {
                return Results.NotFound();
            }

            item.Status = "I";
            await db.SaveChangesAsync();

            var response = new ApiResponse<string>
            {
                Data = "OK",
                Success = true,
                Message = "Garment history item deleted successfully",
                StatusCode = StatusCodes.Status200OK
            };

            return Results.Ok(response);
        });
    }
}

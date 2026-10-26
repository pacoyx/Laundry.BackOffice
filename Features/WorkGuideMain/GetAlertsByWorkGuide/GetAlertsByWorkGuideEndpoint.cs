using Microsoft.EntityFrameworkCore;

public static class GetAlertsByWorkGuideEndpoint
{
    public static void MapGetAlertsByWorkGuide(this IEndpointRouteBuilder app)
    {
        app.MapGet("/getAlertsByWorkGuide/{id}", async (RecepcionDbContext db, int id) =>
        {
            if (id <= 0)
            {
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El id de la guia no es valido",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }

            var alerts = await db.AlertMsgs.Where(x => x.WorkGuideMainId == id).ToListAsync();
            if (alerts.Count == 0)
            {
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "No se encontraron alertas",
                    StatusCode = 404,
                    Success = false
                };
                return Results.Ok(responseValidation);
            }

            var responseAlertaDto = alerts.Select(x => new ResponseAlertsByWorkGuideDto
            {
                Id = x.Id,
                Titulo = x.Titulo,
                TipoAlerta = x.TipoAlerta,
                Mensaje = x.Mensaje
            }).ToList();

            var response = new ApiResponse<List<ResponseAlertsByWorkGuideDto>>()
            {
                Data = responseAlertaDto,
                Message = "Alertas encontradas",
                Success = true,
                StatusCode = 200
            };

            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionConsultas);
    }
}
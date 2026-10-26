public static class CreateAlertWorkGuideEndpoint{
    public static void MapCreateAlertWorkGuide(this IEndpointRouteBuilder app){
        app.MapPost("/createAlert", async (RecepcionDbContext db, RequestCreateAlertMsgDto request) =>
        {
            var alertMsg = new AlertMsg()
            {
                Titulo = request.Titulo,
                TipoAlerta = request.TipoAlerta,
                Mensaje = request.Mensaje,
                WorkGuideMainId = request.WorkGuideMainId              
            };

            db.AlertMsgs.Add(alertMsg);
            await db.SaveChangesAsync();

            var response = new ApiResponse<AlertMsg>()
            {
                Data = alertMsg,
                Message = "Alerta creada con exito",
                Success = true,
                StatusCode = 200
            };
            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
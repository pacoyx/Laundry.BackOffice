public static class DeleteWorkGuideMainEndpoint{
    public static void MapDeleteWorkGuideMain(this IEndpointRouteBuilder app){
        app.MapDelete("/{id}", async (RecepcionDbContext db, int id, IAppLogger<string> logger) =>{
            var workGuideMain = await db.WorkGuideMains.FindAsync(id);
            if (workGuideMain == null){
                return Results.NotFound();
            }
            db.WorkGuideMains.Remove(workGuideMain);
            await db.SaveChangesAsync();
            logger.LogInformacion("guia eliminada con exito" + id.ToString(), "DeleteWorkGuideMainEndpoint");
            return Results.Ok(workGuideMain);
        }).RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
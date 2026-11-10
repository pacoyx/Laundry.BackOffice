public static class PublicContactEndpoints
{
    public static void MapPublicContact(this IEndpointRouteBuilder app)
    {
        app.MapSendSupplierContact();
        app.MapSendJoinUsContact();
        app.MapSendAdvisoryContact();
        app.MapSendSketchDexContact();
    }
}

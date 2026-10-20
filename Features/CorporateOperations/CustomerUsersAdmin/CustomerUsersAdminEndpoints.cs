public static class CustomerUsersAdminEndpoints
{
    public static void MapCustomerUsersAdmin(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/corporate-operations/customer-users")
            .WithTags("CustomerUsersAdmin");

        group.MapGetCustomerUsersAdmin();
        group.MapResetCustomerUserPasswordAdmin();
    }
}

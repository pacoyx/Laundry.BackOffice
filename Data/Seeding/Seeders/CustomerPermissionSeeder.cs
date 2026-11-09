using Microsoft.EntityFrameworkCore;

public class CustomerPermissionSeeder : IApplicationSeeder
{
    private static readonly (string Code, string Description)[] Permissions =
    {
        (CustomerPermissionCodes.GuidesView, "Ver guias de recojo de la empresa"),
        (CustomerPermissionCodes.InvoicesView, "Ver facturas de la empresa"),
        (CustomerPermissionCodes.ReportsView, "Ver reportes de la empresa"),
        (CustomerPermissionCodes.UsersManage, "Administrar usuarios del portal de la empresa"),
        (CustomerPermissionCodes.GarmentsManage, "Registrar y gestionar tickets de ingreso de prendas de la empresa")
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingCodes = await context.CustomerPermissions
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var missing = Permissions.Where(p => !existingCodes.Contains(p.Code));

        foreach (var permission in missing)
        {
            context.CustomerPermissions.Add(new CustomerPermission
            {
                Code = permission.Code,
                Description = permission.Description
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

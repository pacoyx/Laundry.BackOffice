using Microsoft.EntityFrameworkCore;

public class CustomerRolePermissionSeeder : IApplicationSeeder
{
    private static readonly Dictionary<string, string[]> RolePermissionMatrix = new()
    {
        [CustomerRoleNames.AdministradorEmpresa] = CustomerPermissionCodes.All,
        [CustomerRoleNames.Facturacion] = new[]
        {
            CustomerPermissionCodes.InvoicesView, CustomerPermissionCodes.ReportsView
        },
        [CustomerRoleNames.Operaciones] = new[]
        {
            CustomerPermissionCodes.GuidesView
        },
        [CustomerRoleNames.Consulta] = new[]
        {
            CustomerPermissionCodes.GuidesView, CustomerPermissionCodes.InvoicesView, CustomerPermissionCodes.ReportsView
        },
        [CustomerRoleNames.Roperia] = new[]
        {
            CustomerPermissionCodes.GarmentsManage, CustomerPermissionCodes.GuidesView
        }
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var roles = await context.CustomerRoles.ToListAsync(cancellationToken);
        var permissions = await context.CustomerPermissions.ToListAsync(cancellationToken);

        var existingPairs = await context.CustomerRolePermissions
            .Select(x => new { x.CustomerRoleId, x.CustomerPermissionId })
            .ToListAsync(cancellationToken);

        foreach (var (roleName, permissionCodes) in RolePermissionMatrix)
        {
            var role = roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var code in permissionCodes)
            {
                var permission = permissions.FirstOrDefault(x => x.Code == code);
                if (permission is null)
                {
                    continue;
                }

                var alreadyAssigned = existingPairs.Any(x => x.CustomerRoleId == role.Id && x.CustomerPermissionId == permission.Id);
                if (alreadyAssigned)
                {
                    continue;
                }

                context.CustomerRolePermissions.Add(new CustomerRolePermission
                {
                    CustomerRoleId = role.Id,
                    CustomerPermissionId = permission.Id
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

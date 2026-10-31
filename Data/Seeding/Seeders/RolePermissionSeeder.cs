using Microsoft.EntityFrameworkCore;

public class RolePermissionSeeder : IApplicationSeeder
{
    private static readonly Dictionary<string, string[]> RolePermissionMatrix = new()
    {
        [RoleNames.Administrador] = PermissionCodes.All,
        [RoleNames.Recepcion] = new[]
        {
            PermissionCodes.Dashboard, PermissionCodes.Reception,
            PermissionCodes.ReceptionEmision, PermissionCodes.ReceptionRetiro,
            PermissionCodes.ReceptionConsultas, PermissionCodes.ReceptionCaja,
            PermissionCodes.ReceptionGastos, PermissionCodes.ReceptionMaestros,
            PermissionCodes.ReceptionPedidosWeb,
            PermissionCodes.Customers, PermissionCodes.Invoices,
            PermissionCodes.CorporateOperations, PermissionCodes.GuiaRemision
        },
        [RoleNames.RecepcionBasico] = new[]
        {
            PermissionCodes.Dashboard, PermissionCodes.ReceptionEmision,
            PermissionCodes.ReceptionRetiro
        },
        [RoleNames.Compras] = new[]
        {
            PermissionCodes.Dashboard, PermissionCodes.Purchases, PermissionCodes.Suppliers
        },
        [RoleNames.Rrhh] = new[]
        {
            PermissionCodes.Dashboard, PermissionCodes.Employees, PermissionCodes.Payroll
        },
        [RoleNames.Gerencia] = new[]
        {
            PermissionCodes.Dashboard, PermissionCodes.Customers, PermissionCodes.Invoices,
            PermissionCodes.Purchases, PermissionCodes.Expenses, PermissionCodes.Employees,
            PermissionCodes.Payroll, PermissionCodes.Reports, PermissionCodes.CorporateOperations,
            PermissionCodes.GuiaRemision
        },
        [RoleNames.OperacionesCorporativas] = new[]
        {
            PermissionCodes.CorporateOperations
        },
        [RoleNames.Recolector] = new[]
        {
            PermissionCodes.PickupMobile
        }
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var roles = await context.Roles.ToListAsync(cancellationToken);
        var permissions = await context.Permissions.ToListAsync(cancellationToken);

        var existingPairs = await context.RolePermissions
            .Select(x => new { x.RoleId, x.PermissionId })
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

                var alreadyAssigned = existingPairs.Any(x => x.RoleId == role.Id && x.PermissionId == permission.Id);
                if (alreadyAssigned)
                {
                    continue;
                }

                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

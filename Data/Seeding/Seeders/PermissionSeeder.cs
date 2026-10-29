using Microsoft.EntityFrameworkCore;

public class PermissionSeeder : IApplicationSeeder
{
    private static readonly (string Code, string Description)[] Permissions =
    {
        (PermissionCodes.Dashboard, "Acceso al dashboard general"),
        (PermissionCodes.Reception, "Acceso al modulo de Recepcion"),
        (PermissionCodes.ReceptionEmision, "Acceso a Emision de guias en Recepcion"),
        (PermissionCodes.ReceptionRetiro, "Acceso a Retiro de prendas en Recepcion"),
        (PermissionCodes.ReceptionConsultas, "Acceso a Consultas en Recepcion"),
        (PermissionCodes.ReceptionCaja, "Acceso a Caja en Recepcion"),
        (PermissionCodes.ReceptionGastos, "Acceso a Gastos en Recepcion"),
        (PermissionCodes.ReceptionMaestros, "Acceso a Maestros en Recepcion"),
        (PermissionCodes.ReceptionPedidosWeb, "Acceso a Pedidos Web en Recepcion"),
        (PermissionCodes.Customers, "Acceso al modulo de Clientes"),
        (PermissionCodes.Invoices, "Acceso al modulo de Facturacion"),
        (PermissionCodes.Purchases, "Acceso al modulo de Compras"),
        (PermissionCodes.Expenses, "Acceso al modulo de Gastos"),
        (PermissionCodes.Employees, "Acceso al modulo de Recursos Humanos"),
        (PermissionCodes.Payroll, "Acceso al modulo de Planillas"),
        (PermissionCodes.Suppliers, "Acceso al modulo de Proveedores"),
        (PermissionCodes.Reports, "Acceso al modulo de Reportes"),
        (PermissionCodes.Settings, "Acceso a la configuracion del sistema"),
        (PermissionCodes.CorporateOperations, "Acceso al modulo de Operaciones Corporativas"),
        (PermissionCodes.PickupMobile, "Acceso al modulo movil de recojos")
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingCodes = await context.Permissions
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var missing = Permissions.Where(p => !existingCodes.Contains(p.Code));

        foreach (var permission in missing)
        {
            context.Permissions.Add(new Permission
            {
                Code = permission.Code,
                Description = permission.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

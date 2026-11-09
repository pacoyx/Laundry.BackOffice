using Microsoft.EntityFrameworkCore;

public class CustomerRoleSeeder : IApplicationSeeder
{
    private static readonly (string Name, string Description)[] Roles =
    {
        (CustomerRoleNames.AdministradorEmpresa, "Administra usuarios y tiene acceso total al portal de la empresa"),
        (CustomerRoleNames.Facturacion, "Consulta facturas y reportes de la empresa"),
        (CustomerRoleNames.Operaciones, "Consulta guias de recojo de la empresa"),
        (CustomerRoleNames.Consulta, "Consulta guias, facturas y reportes de la empresa"),
        (CustomerRoleNames.Roperia, "Registra el ingreso de prendas de los trabajadores de la empresa para su recojo")
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.CustomerRoles
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var missing = Roles.Where(r => !existingNames.Contains(r.Name));

        foreach (var role in missing)
        {
            context.CustomerRoles.Add(new CustomerRole
            {
                Name = role.Name,
                Description = role.Description,
                IsActive = true
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

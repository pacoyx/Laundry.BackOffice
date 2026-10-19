using Microsoft.EntityFrameworkCore;

// Crea la fila inicial "ope_corporativas" en EmailAccounts (antes vivia en appsettings:Email).
// El password NO se siembra aqui a proposito: este archivo se commitea al repositorio, y
// poner el password real en codigo fuente lo dejaria expuesto en el historial de git igual
// que si siguiera en appsettings.Development.json. Debe completarse una vez desplegado,
// via el endpoint de mantenimiento (PUT /api/email-accounts/{id}).
public class EmailAccountSeeder : IApplicationSeeder
{
    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var exists = await context.EmailAccounts
            .AnyAsync(x => x.Operacion == EmailOperationCodes.CorporateOperations, cancellationToken);

        if (exists)
        {
            return;
        }

        context.EmailAccounts.Add(new EmailAccount
        {
            Operacion = EmailOperationCodes.CorporateOperations,
            Provider = "Smtp",
            Host = "mail.privateemail.com",
            Port = 587,
            UserName = "atencionalcliente@lavanderiakyo.com",
            Password = string.Empty,
            EnableSsl = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}

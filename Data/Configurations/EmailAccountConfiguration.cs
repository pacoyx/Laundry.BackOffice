using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmailAccountConfiguration : IEntityTypeConfiguration<EmailAccount>
{
    public void Configure(EntityTypeBuilder<EmailAccount> builder)
    {
        builder.ToTable("EmailAccounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Operacion).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Operacion).IsUnique();

        builder.Property(x => x.Provider).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Host).IsRequired().HasMaxLength(200);
        builder.Property(x => x.UserName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Password).IsRequired().HasMaxLength(500);
    }
}

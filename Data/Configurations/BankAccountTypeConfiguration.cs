using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BankAccountTypeConfiguration : IEntityTypeConfiguration<BankAccountType>
{
    public void Configure(EntityTypeBuilder<BankAccountType> builder)
    {
        builder.ToTable("BankAccountTypes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

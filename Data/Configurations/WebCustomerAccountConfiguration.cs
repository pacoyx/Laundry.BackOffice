using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WebCustomerAccountConfiguration : IEntityTypeConfiguration<WebCustomerAccount>
{
    public void Configure(EntityTypeBuilder<WebCustomerAccount> builder)
    {
        builder.ToTable("WebCustomerAccounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Phone).IsRequired().HasMaxLength(20);

        builder.HasIndex(x => x.Phone).IsUnique();
        builder.HasIndex(x => x.CustomerId).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(x => x.CustomerType).HasDefaultValue(CustomerType.Individual);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.PortalEnabled).HasDefaultValue(false);
        builder.Property(x => x.InternalCode).HasMaxLength(20);
        builder.Property(x => x.Origin).HasDefaultValue(CustomerOrigin.Recepcion);

        builder.HasOne(x => x.District)
            .WithMany()
            .HasForeignKey(x => x.DistrictId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BillingProfile)
            .WithOne(x => x.Customer)
            .HasForeignKey<CustomerBillingProfile>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

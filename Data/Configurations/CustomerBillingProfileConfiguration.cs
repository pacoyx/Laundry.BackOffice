using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerBillingProfileConfiguration : IEntityTypeConfiguration<CustomerBillingProfile>
{
    public void Configure(EntityTypeBuilder<CustomerBillingProfile> builder)
    {
        builder.ToTable("CustomerBillingProfiles");
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.TaxId).IsRequired().HasMaxLength(20);
        builder.Property(x => x.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CommercialName).HasMaxLength(200);
        builder.Property(x => x.FiscalAddress).IsRequired().HasMaxLength(300);
        builder.Property(x => x.BillingEmail).HasMaxLength(200);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
        builder.Property(x => x.RemindersEnabled).HasDefaultValue(true);
        builder.Property(x => x.RemindersSuppressedReason).HasMaxLength(300);
    }
}

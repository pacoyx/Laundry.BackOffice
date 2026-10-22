using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Serie).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Numero).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.ReminderSuppressed).HasDefaultValue(false);
        builder.Property(x => x.ReminderSuppressedReason).HasMaxLength(300);

        builder.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TypePayment).IsRequired().HasMaxLength(10);
        builder.Property(x => x.DaysCredit).IsRequired();

        builder.Property(x => x.CurrencyId).IsRequired().HasMaxLength(3);
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 4);

        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.Igv).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.Balance).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.Serie, x.Numero }).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);
    }
}

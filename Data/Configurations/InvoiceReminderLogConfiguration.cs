using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceReminderLogConfiguration : IEntityTypeConfiguration<InvoiceReminderLog>
{
    public void Configure(EntityTypeBuilder<InvoiceReminderLog> builder)
    {
        builder.ToTable("InvoiceReminderLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkipReason).HasMaxLength(300);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.Recipients).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.CustomerId, x.RunAt });

        builder.HasOne(x => x.ScheduledEmailNotification)
            .WithMany()
            .HasForeignKey(x => x.ScheduledEmailNotificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.InvoiceReminderLog)
            .HasForeignKey(x => x.InvoiceReminderLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

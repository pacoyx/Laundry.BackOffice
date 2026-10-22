using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ScheduledEmailNotificationConfiguration : IEntityTypeConfiguration<ScheduledEmailNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledEmailNotification> builder)
    {
        builder.ToTable("ScheduledEmailNotifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.CronExpression).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SubjectTemplate).IsRequired().HasMaxLength(300);
        builder.Property(x => x.BodyTemplate).IsRequired();
        builder.Property(x => x.DriverName).HasMaxLength(150);
        builder.Property(x => x.DriverDocument).HasMaxLength(20);
        builder.Property(x => x.SignatureName).HasMaxLength(150);
        builder.Property(x => x.SignaturePosition).HasMaxLength(100);
        builder.Property(x => x.SignaturePhone).HasMaxLength(30);
        builder.Property(x => x.IsEnabled).HasDefaultValue(false);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.Recipients)
            .WithOne(x => x.ScheduledEmailNotification)
            .HasForeignKey(x => x.ScheduledEmailNotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Executions)
            .WithOne(x => x.ScheduledEmailNotification)
            .HasForeignKey(x => x.ScheduledEmailNotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

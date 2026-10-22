using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ScheduledEmailExecutionConfiguration : IEntityTypeConfiguration<ScheduledEmailExecution>
{
    public void Configure(EntityTypeBuilder<ScheduledEmailExecution> builder)
    {
        builder.ToTable("ScheduledEmailExecutions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.Recipients).IsRequired().HasMaxLength(1000);

        builder.HasIndex(x => new { x.ScheduledEmailNotificationId, x.ExecutedAt });
    }
}

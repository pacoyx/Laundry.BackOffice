using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ScheduledEmailRecipientConfiguration : IEntityTypeConfiguration<ScheduledEmailRecipient>
{
    public void Configure(EntityTypeBuilder<ScheduledEmailRecipient> builder)
    {
        builder.ToTable("ScheduledEmailRecipients");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);

        // Un mismo correo no puede repetirse ni dentro del mismo tipo (TO/BCC) ni cruzado entre ambos.
        builder.HasIndex(x => new { x.ScheduledEmailNotificationId, x.Email }).IsUnique();
    }
}

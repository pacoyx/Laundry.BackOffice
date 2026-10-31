using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SharedDocumentRecipientConfiguration : IEntityTypeConfiguration<SharedDocumentRecipient>
{
    public void Configure(EntityTypeBuilder<SharedDocumentRecipient> builder)
    {
        builder.ToTable("SharedDocumentRecipients");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SharedDocumentId, x.UserId }).IsUnique();

        builder.HasOne(x => x.SharedDocument)
            .WithMany(x => x.Recipients)
            .HasForeignKey(x => x.SharedDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

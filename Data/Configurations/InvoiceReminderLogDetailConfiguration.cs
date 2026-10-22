using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceReminderLogDetailConfiguration : IEntityTypeConfiguration<InvoiceReminderLogDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceReminderLogDetail> builder)
    {
        builder.ToTable("InvoiceReminderLogDetails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Balance).HasPrecision(18, 2);

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

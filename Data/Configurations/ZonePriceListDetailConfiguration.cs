using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ZonePriceListDetailConfiguration : IEntityTypeConfiguration<ZonePriceListDetail>
{
    public void Configure(EntityTypeBuilder<ZonePriceListDetail> builder)
    {
        builder.ToTable("ZonePriceListDetails");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ZonePriceListId, x.ServiceItemId }).IsUnique();

        builder.HasOne(x => x.ServiceItem)
            .WithMany()
            .HasForeignKey(x => x.ServiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ZonePriceListConfiguration : IEntityTypeConfiguration<ZonePriceList>
{
    public void Configure(EntityTypeBuilder<ZonePriceList> builder)
    {
        builder.ToTable("ZonePriceLists");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => new { x.ZoneId, x.Version }).IsUnique();

        // Garantiza que una zona no pueda tener mas de una tarifa activa a la vez.
        builder.HasIndex(x => x.ZoneId)
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_ZonePriceLists_ZoneId_ActiveOnly");

        builder.HasOne(x => x.Zone)
            .WithMany()
            .HasForeignKey(x => x.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.ZonePriceList)
            .HasForeignKey(x => x.ZonePriceListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

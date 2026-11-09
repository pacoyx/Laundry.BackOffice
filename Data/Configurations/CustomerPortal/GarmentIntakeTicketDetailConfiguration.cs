using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GarmentIntakeTicketDetailConfiguration : IEntityTypeConfiguration<GarmentIntakeTicketDetail>
{
    public void Configure(EntityTypeBuilder<GarmentIntakeTicketDetail> builder)
    {
        builder.ToTable("GarmentIntakeTicketDetails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Observations).HasMaxLength(500);

        builder.HasOne(x => x.Worker)
            .WithMany()
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceItem)
            .WithMany()
            .HasForeignKey(x => x.ServiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WebOrderDetailConfiguration : IEntityTypeConfiguration<WebOrderDetail>
{
    public void Configure(EntityTypeBuilder<WebOrderDetail> builder)
    {
        builder.ToTable("WebOrderDetails");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ServiceItem)
            .WithMany()
            .HasForeignKey(x => x.ServiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

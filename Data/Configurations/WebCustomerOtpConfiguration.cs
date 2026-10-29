using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WebCustomerOtpConfiguration : IEntityTypeConfiguration<WebCustomerOtp>
{
    public void Configure(EntityTypeBuilder<WebCustomerOtp> builder)
    {
        builder.ToTable("WebCustomerOtps");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Phone).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CodeHash).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => new { x.Phone, x.IsUsed });
    }
}

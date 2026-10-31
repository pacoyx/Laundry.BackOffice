using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FixedAssetCategoryConfiguration : IEntityTypeConfiguration<FixedAssetCategory>
{
    public void Configure(EntityTypeBuilder<FixedAssetCategory> builder)
    {
        builder.ToTable("FixedAssetCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

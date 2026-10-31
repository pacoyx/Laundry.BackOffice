using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FixedAssetReassignmentConfiguration : IEntityTypeConfiguration<FixedAssetReassignment>
{
    public void Configure(EntityTypeBuilder<FixedAssetReassignment> builder)
    {
        builder.ToTable("FixedAssetReassignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(300);

        builder.HasOne(x => x.PreviousBranch)
            .WithMany()
            .HasForeignKey(x => x.PreviousBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NewBranch)
            .WithMany()
            .HasForeignKey(x => x.NewBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreviousResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.PreviousResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NewResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.NewResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

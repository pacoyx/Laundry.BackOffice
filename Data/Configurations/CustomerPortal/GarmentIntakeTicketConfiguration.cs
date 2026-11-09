using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GarmentIntakeTicketConfiguration : IEntityTypeConfiguration<GarmentIntakeTicket>
{
    public void Configure(EntityTypeBuilder<GarmentIntakeTicket> builder)
    {
        builder.ToTable("GarmentIntakeTickets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).IsRequired().HasMaxLength(40);
        builder.Property(x => x.Status).HasDefaultValue(GarmentIntakeTicketStatus.Pending);
        builder.Property(x => x.VoidedReason).HasMaxLength(500);

        builder.HasIndex(x => x.TicketNumber).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.Status });

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByCustomerUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByCustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VoidedByCustomerUser)
            .WithMany()
            .HasForeignKey(x => x.VoidedByCustomerUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PickupGuide)
            .WithMany()
            .HasForeignKey(x => x.PickupGuideId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.Ticket)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.Ticket)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

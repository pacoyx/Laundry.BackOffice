using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserAccessScheduleConfiguration : IEntityTypeConfiguration<UserAccessSchedule>
{
    public void Configure(EntityTypeBuilder<UserAccessSchedule> builder)
    {
        builder.ToTable("UserAccessSchedules");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.DayOfWeek });
    }
}

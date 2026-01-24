namespace DevHabit.Infrastructure.Database.Configurations;

public class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasMaxLength(500);
        builder.Property(h => h.Name).HasMaxLength(100);
        builder.Property(h => h.Description).HasMaxLength(500);

        builder.OwnsOne(h => h.Frequency);
        builder.OwnsOne(h => h.Targets, targetBuilder =>
        {
            targetBuilder.Property(t => t.Units).HasMaxLength(100);
        });

        builder.OwnsOne(h => h.Milestone);
    }
}

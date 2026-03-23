namespace DevHabit.Infrastructure.Database.Configurations;

public class HabitTagConfigurations : IEntityTypeConfiguration<HabitTag>
{
    public void Configure(EntityTypeBuilder<HabitTag> builder)
    {
        builder.HasKey(ht => new { ht.HabitId, ht.TagId });
        
        builder.Property(ht => ht.HabitId).HasMaxLength(500);
        builder.Property(ht => ht.TagId).HasMaxLength(500);
        
        builder.HasOne(ht => ht.Habit)
            .WithMany(h => h.HabitTags)
            .HasForeignKey(ht => ht.HabitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ht => ht.Tag)
            .WithMany()
            .HasForeignKey(ht => ht.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

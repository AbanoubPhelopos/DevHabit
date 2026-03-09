using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevHabit.Infrastructure.Database.Configurations;

public class HabitTagConfigurations : IEntityTypeConfiguration<HabitTag>
{
    public void Configure(EntityTypeBuilder<HabitTag> builder)
    {
        builder.HasKey(ht => new { ht.HabitId, ht.TagId });
        
        builder.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(ht => ht.TagId);
        
        builder.HasOne<Habit>()
            .WithMany(ht => ht.HabitTags)
            .HasForeignKey(ht => ht.HabitId);
    }
}

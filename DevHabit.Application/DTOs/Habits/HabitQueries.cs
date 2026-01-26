namespace DevHabit.Application.DTOs.Habits;

public static class HabitQueries
{
    public static Expression<Func<Habit, HabitDto>> ProjectToDto()
    {
        return h=> new HabitDto(
            Id:h.Id,
            Name:h.Name,
            Description:h.Description,
            Type:h.Type,
            Frequency:new FrequencyDto(
                FrequencyType:h.Frequency.FrequencyType,
                TimesPerPeriod:h.Frequency.TimesPerPeriod
            ),
            Targets:new TargetDto(
                Value:h.Targets.Value,
                Units:h.Targets.Units
            ),
            Status:h.Status,
            IsArchived:h.IsArchived,
            EndDate:h.EndDate,
            Milestone:h.Milestone == null ? null : new MilestoneDto(
                Target:h.Milestone.Target,
                Current:h.Milestone.Current
            ),
            CreatedAtUtc:h.CreatedAtUtc,
            UpdatedAtUtc:h.UpdatedAtUtc,
            LastCompletedAtUtc:h.LastCompletedAtUtc
        );
    }
}

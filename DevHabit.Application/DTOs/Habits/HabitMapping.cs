namespace DevHabit.Application.DTOs.Habits;

public static class HabitMapping
{
    public static HabitDto ToDto(this Habit habit)
    {
        var dto = new HabitDto(
            Id: habit.Id,
            Name: habit.Name,
            Description: habit.Description,
            Type: habit.Type,
            Frequency: new FrequencyDto(
                FrequencyType: habit.Frequency.FrequencyType,
                TimesPerPeriod: habit.Frequency.TimesPerPeriod
            ),
            Targets: new TargetDto(
                Value: habit.Targets.Value,
                Units: habit.Targets.Units
            ),
            Status: habit.Status,
            IsArchived: habit.IsArchived,
            EndDate: habit.EndDate,
            Milestone: habit.Milestone == null ? null : new MilestoneDto(
                Target: habit.Milestone.Target,
                Current: habit.Milestone.Current
            ),
            CreatedAtUtc: habit.CreatedAtUtc,
            UpdatedAtUtc: habit.UpdatedAtUtc,
            LastCompletedAtUtc: habit.LastCompletedAtUtc
        );
        return dto;
    }
    
    public static Habit ToEntity(this CreateHabitDto createHabitDto)
    {
        return new Habit
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = createHabitDto.Name,
            Description = createHabitDto.Description,
            Type = createHabitDto.Type,
            Frequency = new Frequency
            {
                FrequencyType = createHabitDto.Frequency.FrequencyType,
                TimesPerPeriod = createHabitDto.Frequency.TimesPerPeriod
            },
            Targets = new Target
            {
                Value = createHabitDto.Targets.Value,
                Units = createHabitDto.Targets.Units
            },
            Status = HabitStatus.OnGoing,
            IsArchived = false,
            EndDate = createHabitDto.EndDate,
            Milestone = createHabitDto.Milestone == null ? null : new Milestone
            {
                Target = createHabitDto.Milestone.Target,
                Current = 0 // Initialize current progress to 0 
            },
            CreatedAtUtc = DateTime.UtcNow
        };
    }
    
    public static void UpdateFromDto(this Habit habit, UpdateHabitDto updateHabitDto)
    {
        habit.Name = updateHabitDto.Name;
        habit.Description = updateHabitDto.Description;
        habit.Type = updateHabitDto.Type;
        
        habit.Frequency = new Frequency
        {
            FrequencyType = updateHabitDto.Frequency.FrequencyType,
            TimesPerPeriod = updateHabitDto.Frequency.TimesPerPeriod
        };
        habit.Targets = new Target
        {
            Value = updateHabitDto.Targets.Value,
            Units = updateHabitDto.Targets.Units
        };
        
        habit.EndDate = updateHabitDto.EndDate;
        
        if (updateHabitDto.Milestone != null)
        {
            habit.Milestone ??= new Milestone();
            habit.Milestone.Target = updateHabitDto.Milestone.Target;
        }
        
        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
}

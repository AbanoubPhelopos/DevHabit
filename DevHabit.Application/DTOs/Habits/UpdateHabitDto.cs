namespace DevHabit.Application.DTOs.Habits;

public sealed record UpdateHabitDto(
    
    string Name,
    string? Description,
    
    HabitType  Type,
    FrequencyDto Frequency,
    TargetDto Targets,
    
    DateOnly EndDate,
    UpdateMilestoneDto? Milestone
);

public sealed record UpdateMilestoneDto(
    int Target
);

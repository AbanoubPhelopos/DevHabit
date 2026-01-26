
namespace DevHabit.Application.DTOs.Habits;

public sealed record CreateHabitDto(
    
    string Name,
    string? Description,
    
    HabitType  Type,
    FrequencyDto Frequency,
    TargetDto Targets,
    
    DateOnly EndDate,
    MilestoneDto? Milestone

    );

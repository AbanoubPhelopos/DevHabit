using DevHabit.Domain.Enums;

namespace DevHabit.Application.DTOs.Habits;

public sealed record HabitCollectionsDto
(
    List<HabitDto> Data
);


public sealed record HabitDto
(
    string Id,
    string Name,
    string? Description,
    
    HabitType  Type,
    FrequencyDto Frequency,
    TargetDto Targets,
    HabitStatus Status,
    
    bool IsArchived,
    
    DateOnly EndDate,
    MilestoneDto? Milestone,
    
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    
    DateTime LastCompletedAtUtc
);



public sealed record FrequencyDto
(
    FrequencyType FrequencyType,
    int TimesPerPeriod
);

public sealed record TargetDto(
    int Value,
    string Units
);

public sealed record MilestoneDto(
    int Target,
    int Current
);

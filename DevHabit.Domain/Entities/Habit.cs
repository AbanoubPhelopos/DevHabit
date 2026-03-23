namespace DevHabit.Domain.Entities;

public sealed class Habit : BaseAuditEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
    
    public HabitType  Type { get; set; } 
    public Frequency Frequency { get; set; }
    public Target Targets { get; set; }
    public HabitStatus Status { get; set; } = HabitStatus.OnGoing;
    
    public bool IsArchived { get; set; }
    
    public DateOnly EndDate { get; set; }
    public Milestone? Milestone { get; set; }
    
    public DateTime LastCompletedAtUtc { get; set; }

    public  List<HabitTag> HabitTags { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}


public sealed class Frequency
{
    public FrequencyType FrequencyType { get; set; }
    public int TimesPerPeriod { get; set; }
}

public sealed class Target
{
    public int Value { get; set; }
    public string Units { get; set; } 
}

public sealed class Milestone
{
    public int Target { get; set; }
    public int Current { get; set; }
}

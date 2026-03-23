namespace DevHabit.Domain.Entities;

public sealed class Tag : BaseAuditEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
}

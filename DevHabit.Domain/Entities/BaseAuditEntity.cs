namespace DevHabit.Domain.Entities;

public abstract class BaseAuditEntity
{
    public string Id { get; set; } = string.Empty;
    
    public EntityStatus RecordStatus { get; set; } = EntityStatus.Active;
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public string? StatusNotes { get; set; }
}

public enum EntityStatus
{
    Inactive = 0,
    Active = 1,
    Deleted = 99
}

namespace DevHabit.Application.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityName, string entityId)
        : base($"Entity '{entityName}' with ID '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
    }
}

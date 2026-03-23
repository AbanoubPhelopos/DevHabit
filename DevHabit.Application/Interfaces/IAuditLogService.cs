namespace DevHabit.Application.Interfaces;

public interface IAuditLogService
{
    void Log(string entityName, string entityId, string action, string? oldValues = null, string? newValues = null);
}

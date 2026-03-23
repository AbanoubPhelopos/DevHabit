using System.Text.Json;
using System.Text.Json.Serialization;
using DevHabit.Application.Interfaces;
using DevHabit.Domain.Entities;
using DevHabit.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DevHabit.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly RequestScopedService _scopedService;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    public AuditLogService(ApplicationDbContext context, RequestScopedService scopedService)
    {
        _context = context;
        _scopedService = scopedService;
    }

    public void Log(string entityName, string entityId, string action, string? oldValues = null, string? newValues = null)
    {
        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = _scopedService.UserId,
            Timestamp = DateTime.UtcNow,
            TablePartition = entityName.ToLower()
        };

        _context.AuditLogs.Add(auditLog);
    }

    public void LogChanges(
        string entityName,
        string entityId,
        string action,
        object? originalEntity = null,
        object? currentEntity = null)
    {
        if (originalEntity == null && currentEntity == null)
        {
            Log(entityName, entityId, action);
            return;
        }

        var propertyChanges = new Dictionary<string, object>();
        var originalType = originalEntity?.GetType() ?? currentEntity!.GetType();

        var properties = originalType.GetProperties()
            .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string));

        foreach (var property in properties)
        {
            var originalValue = originalEntity != null ? property.GetValue(originalEntity) : null;
            var currentValue = currentEntity != null ? property.GetValue(currentEntity) : null;

            if (!Equals(originalValue, currentValue))
            {
                propertyChanges[property.Name] = new
                {
                    Old = originalValue,
                    New = currentValue
                };
            }
        }

        if (propertyChanges.Count == 0)
        {
            return;
        }

        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = originalEntity != null ? JsonSerializer.Serialize(GetSimpleObject(originalEntity, propertyChanges.Keys.ToList()), JsonOptions) : null,
            NewValues = currentEntity != null ? JsonSerializer.Serialize(GetSimpleObject(currentEntity, propertyChanges.Keys.ToList()), JsonOptions) : null,
            UserId = _scopedService.UserId,
            Timestamp = DateTime.UtcNow,
            TablePartition = entityName.ToLower()
        };

        _context.AuditLogs.Add(auditLog);
    }

    public void LogEntityEntry(EntityEntry entry, string entityId, string action)
    {
        if (entry.State == EntityState.Modified)
        {
            var modifiedProperties = entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(p => p.Metadata.Name, p => new
                {
                    Old = p.OriginalValue,
                    New = p.CurrentValue
                });

            if (modifiedProperties.Count == 0)
            {
                return;
            }

            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = action,
                OldValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(k => k.Key, k => k.Value.Old), JsonOptions),
                NewValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(k => k.Key, k => k.Value.New), JsonOptions),
                UserId = _scopedService.UserId,
                Timestamp = DateTime.UtcNow,
                TablePartition = entry.Entity.GetType().Name.ToLower()
            };

            _context.AuditLogs.Add(auditLog);
        }
        else if (entry.State == EntityState.Added)
        {
            var currentValues = entry.Properties
                .Where(p => p.CurrentValue != null)
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = action,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(currentValues, JsonOptions),
                UserId = _scopedService.UserId,
                Timestamp = DateTime.UtcNow,
                TablePartition = entry.Entity.GetType().Name.ToLower()
            };

            _context.AuditLogs.Add(auditLog);
        }
        else if (entry.State == EntityState.Deleted)
        {
            var originalValues = entry.Properties
                .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = action,
                OldValues = JsonSerializer.Serialize(originalValues, JsonOptions),
                NewValues = null,
                UserId = _scopedService.UserId,
                Timestamp = DateTime.UtcNow,
                TablePartition = entry.Entity.GetType().Name.ToLower()
            };

            _context.AuditLogs.Add(auditLog);
        }
    }

    private static Dictionary<string, object?>? GetSimpleObject(object entity, List<string> propertyNames)
    {
        var type = entity.GetType();
        var dictionary = new Dictionary<string, object?>();

        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                dictionary[propertyName] = property.GetValue(entity);
            }
        }

        return dictionary;
    }
}

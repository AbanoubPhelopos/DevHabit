using DevHabit.Application.Interfaces;
using DevHabit.Domain.Entities;
using DevHabit.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly RequestScopedService _scopedService;

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
        _context.SaveChanges();
    }
}

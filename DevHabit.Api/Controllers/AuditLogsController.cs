using DevHabit.Application.DTOs.AuditLogs;
using DevHabit.Domain.Entities;
using DevHabit.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("audit-logs")]
public sealed class AuditLogsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditLogListDto>> GetAuditLogs(
        string? entityName = null,
        string? entityId = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var auditLogs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.OldValues,
                a.NewValues,
                a.UserId,
                a.UserName,
                a.IpAddress,
                a.Timestamp,
                a.TablePartition))
            .ToListAsync(cancellationToken);

        return Ok(new AuditLogListDto(auditLogs, totalCount, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(long id, CancellationToken cancellationToken)
    {
        var auditLog = await dbContext.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (auditLog == null)
        {
            return NotFound();
        }

        return Ok(new AuditLogDto(
            auditLog.Id,
            auditLog.EntityName,
            auditLog.EntityId,
            auditLog.Action,
            auditLog.OldValues,
            auditLog.NewValues,
            auditLog.UserId,
            auditLog.UserName,
            auditLog.IpAddress,
            auditLog.Timestamp,
            auditLog.TablePartition));
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<ActionResult<List<AuditLogDto>>> GetEntityAuditLogs(
        string entityName,
        string entityId,
        CancellationToken cancellationToken)
    {
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogDto(
                a.Id,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.OldValues,
                a.NewValues,
                a.UserId,
                a.UserName,
                a.IpAddress,
                a.Timestamp,
                a.TablePartition))
            .ToListAsync(cancellationToken);

        return Ok(auditLogs);
    }
}

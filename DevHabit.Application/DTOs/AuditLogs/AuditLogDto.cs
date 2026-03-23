namespace DevHabit.Application.DTOs.AuditLogs;

public sealed record AuditLogDto(
    long Id,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? UserId,
    string? UserName,
    string? IpAddress,
    DateTime Timestamp,
    string TablePartition
);

public sealed record AuditLogListDto(
    List<AuditLogDto> Data,
    int TotalCount,
    int Page,
    int PageSize
);

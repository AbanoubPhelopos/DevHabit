namespace DevHabit.Application.Interfaces;

public sealed class RequestScopedService
{
    public string? UserId { get; set; }
    public string? StoreId { get; set; }
    public string? TenantId { get; set; }
}

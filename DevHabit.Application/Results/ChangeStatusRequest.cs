namespace DevHabit.Application.Results;

public sealed class ChangeStatusRequest
{
    public int Status { get; set; }
    public string? Notes { get; set; }
}

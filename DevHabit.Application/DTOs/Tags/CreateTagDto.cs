namespace DevHabit.Application.DTOs.Tags;

public sealed record CreateTagDto(
    string Name,
    string? Description
);

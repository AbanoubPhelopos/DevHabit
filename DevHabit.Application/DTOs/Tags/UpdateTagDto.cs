namespace DevHabit.Application.DTOs.Tags;

public sealed record UpdateTagDto(
    string Name,
    string? Description
);

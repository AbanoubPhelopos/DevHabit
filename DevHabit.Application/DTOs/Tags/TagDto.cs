namespace DevHabit.Application.DTOs.Tags;

public sealed record TagCollection(
    List<TagDto> Data
);


public sealed record TagDto(
    string Id,
    string Name,
    string Description,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

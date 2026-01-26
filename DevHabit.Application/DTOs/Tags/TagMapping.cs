namespace DevHabit.Application.DTOs.Tags;

public static class TagMapping
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto(
            tag.Id,
            tag.Name,
            tag.Description ?? string.Empty,
            tag.CreatedAtUtc,
            tag.UpdatedAtUtc
        );
    }

    public static Tag ToEntity(this CreateTagDto createTagDto)
    {
        return new Tag
        {
            Id = $"t_{Guid.CreateVersion7()}",
            Name = createTagDto.Name,
            Description = createTagDto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
    
    public static void UpdateEntity(this Tag tag, UpdateTagDto updateTagDto)
    {
        tag.Name = updateTagDto.Name;
        tag.Description = updateTagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}

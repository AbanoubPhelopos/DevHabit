namespace DevHabit.Application.DTOs.Tags;

public static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToTagDto()
    {
        return t => new TagDto(
            t.Id,
            t.Name,
            t.Description??string.Empty,
            t.CreatedAtUtc,
            t.UpdatedAtUtc
        );
    }
}

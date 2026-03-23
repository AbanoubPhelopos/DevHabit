using DevHabit.Application.DTOs.HabitTags;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits/{habitId}/tags")]
public class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
        
    [HttpPut]
    public async Task<ActionResult> UpsertHabitTag(string habitId, UpsertHabitTagsDto upsertHabitTagsDto, CancellationToken cancellationToken)
    {
        Habit? habit = await _dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId, cancellationToken);
        if (habit == null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();
        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        {
            return NoContent();
        }
        
        List<string> existingTagIds = await _dbContext.Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        
        if(existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more provided tag IDs do not exist.");
        }
        
        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));
        
        string[] tagIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();

        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc =  DateTime.UtcNow
        }));
            
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId, CancellationToken cancellationToken)
    {
        HabitTag? habitTag = await _dbContext.HabitTags.Where(ht=> ht.HabitId == habitId && ht.TagId == tagId).FirstOrDefaultAsync(cancellationToken);
        
        if (habitTag == null)
        {
            return NotFound();
        }
        
        _dbContext.HabitTags.Remove(habitTag);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }

}

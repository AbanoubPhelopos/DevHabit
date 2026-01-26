namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HabitCollectionsDto>>> GetHabits(CancellationToken cancellationToken)
    {
        List<HabitDto> habits = await dbContext.Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync(cancellationToken);
        
        HabitCollectionsDto result = new(habits);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id, CancellationToken cancellationToken)
    {
        HabitDto habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (habit == null) 
        {
            return NotFound();
        }
        
        return Ok(habit);
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateHabit([FromBody] CreateHabitDto createHabitDto, CancellationToken cancellationToken)
    {
        Habit habit = createHabitDto.ToEntity();
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        HabitDto habitDto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habitDto);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h=>h.Id == id,cancellationToken);

        if (habit is null)
        {
            return NotFound();
        }
        
        habit.UpdateFromDto(updateHabitDto);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, [FromBody] JsonPatchDocument<HabitDto> patchDoc, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h=>h.Id == id,cancellationToken);
        if (habit is null)
        {
            return NotFound();
        }
        
        HabitDto habitDto = habit.ToDto();
        patchDoc.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }
        
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    
    [HttpDelete]
    public async Task<ActionResult> DeleteHabit(string id, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h=>h.Id == id,cancellationToken);
        
        if (habit is null)
        {
            return NotFound();
        }
        
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }
}

using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public class TagController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagCollection>> GetTags(CancellationToken cancellationToken)
    {
        List<TagDto> tags = await dbContext.Tags
            .Select(TagQueries.ProjectToTagDto())
            .ToListAsync(cancellationToken);
        
        TagCollection result = new(tags);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id, CancellationToken cancellationToken)
    {
        TagDto tag = await dbContext.Tags.
            Where(t => t.Id == id).
            Select(TagQueries.ProjectToTagDto()).
            FirstOrDefaultAsync(cancellationToken);
        
        if (tag == null)
        {
            return NotFound();
        }
        
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTag([FromBody] CreateTagDto createTagDto,
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory, CancellationToken cancellationToken)
    {

        ValidationResult validationResult = await validator.ValidateAsync(createTagDto,cancellationToken);
        
        if(!validationResult.IsValid)
        {
            ProblemDetails problems = problemDetailsFactory.CreateProblemDetails(HttpContext,
                StatusCodes.Status400BadRequest);
            
            return BadRequest(problems);
        }
        
        Tag existingTag = await dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name == createTagDto.Name, cancellationToken);
        
        if (existingTag != null)
        {
            return Conflict($"A tag with the name '{createTagDto.Name}' already exists.");
        }
        
        Tag tag = createTagDto.ToEntity();
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        TagDto tagDto = tag.ToDto();
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, [FromBody] UpdateTagDto updateTagDto,
        CancellationToken cancellationToken)
    {
        Tag existingTag = await dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name == updateTagDto.Name, cancellationToken);
        
        if (existingTag != null)
        {
            return Conflict($"A tag with the name '{updateTagDto.Name}' already exists.");
        }
        
        Tag tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tag is null)
        {
            return NotFound();
        }
        tag.UpdateEntity(updateTagDto);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id, CancellationToken cancellationToken)
    {
        Tag tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tag is null)
        {
            return NotFound();
        }
        
        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

using DevHabit.Application.DTOs.Tags;
using DevHabit.Application.Results;
using DevHabit.Application.Services.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public sealed class TagController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagCollection>> GetTags(CancellationToken cancellationToken)
    {
        ServiceResult<List<TagDto>> result = await tagService.GetAllAsync(cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode);
        }

        return Ok(new TagCollection(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id, CancellationToken cancellationToken)
    {
        ServiceResult<TagDto> result = await tagService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTag(
        [FromBody] CreateTagDto createTagDto,
        IValidator<CreateTagDto> validator,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(createTagDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        ServiceResult<TagDto> result = await tagService.CreateAsync(createTagDto, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, result.Message);
        }

        return CreatedAtAction(nameof(GetTag), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(
        string id,
        [FromBody] UpdateTagDto updateTagDto,
        CancellationToken cancellationToken)
    {
        ServiceResult<TagDto> result = await tagService.UpdateAsync(id, updateTagDto, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id, CancellationToken cancellationToken)
    {
        ServiceResult<bool> result = await tagService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return NoContent();
    }
}

using DevHabit.Application.DTOs.Habits;
using DevHabit.Application.Results;
using DevHabit.Application.Services.Abstractions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(IHabitService habitService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitCollectionsDto>> GetHabits(CancellationToken cancellationToken)
    {
        ServiceResult<List<HabitDto>> result = await habitService.GetAllAsync(cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode);
        }

        return Ok(new HabitCollectionsDto(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDtoWithTags>> GetHabit(string id, CancellationToken cancellationToken)
    {
        ServiceResult<HabitDtoWithTags> result = await habitService.GetWithTagsAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult> CreateHabit([FromBody] CreateHabitDto createHabitDto, CancellationToken cancellationToken)
    {
        ServiceResult<HabitDto> result = await habitService.CreateAsync(createHabitDto, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, result.Message);
        }

        return CreatedAtAction(nameof(GetHabit), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto, CancellationToken cancellationToken)
    {
        ServiceResult<HabitDto> result = await habitService.UpdateAsync(id, updateHabitDto, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, [FromBody] JsonPatchDocument<HabitDto> patchDoc, CancellationToken cancellationToken)
    {
        ServiceResult<HabitDto> habitResult = await habitService.GetByIdAsync(id, cancellationToken);

        if (habitResult.IsFailure)
        {
            return NotFound();
        }

        HabitDto habitDto = habitResult.Data!;
        patchDoc.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        var updateDto = new UpdateHabitDto(
            habitDto.Name,
            habitDto.Description,
            habitDto.Type,
            new FrequencyDto(habitDto.Frequency.FrequencyType, habitDto.Frequency.TimesPerPeriod),
            new TargetDto(habitDto.Targets.Value, habitDto.Targets.Units),
            habitDto.EndDate,
            habitDto.Milestone != null ? new UpdateMilestoneDto(habitDto.Milestone.Target) : null
        );

        ServiceResult<HabitDto> updateResult = await habitService.UpdateAsync(id, updateDto, cancellationToken);

        if (updateResult.IsFailure)
        {
            return StatusCode(updateResult.StatusCode, updateResult.Message);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id, CancellationToken cancellationToken)
    {
        ServiceResult<bool> result = await habitService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> ChangeHabitStatus(string id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<string> result = await habitService.ChangeStatusAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }
}

using DevHabit.Application.DTOs.Habits;
using DevHabit.Application.Results;

namespace DevHabit.Application.Services.Abstractions;

public interface IHabitService
{
    Task<ServiceResult<HabitDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ServiceResult<List<HabitDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<List<HabitDto>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<HabitDto>> CreateAsync(CreateHabitDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<HabitDto>> UpdateAsync(string id, UpdateHabitDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> ChangeStatusAsync(string id, ChangeStatusRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<HabitDtoWithTags>> GetWithTagsAsync(string id, CancellationToken cancellationToken = default);
}

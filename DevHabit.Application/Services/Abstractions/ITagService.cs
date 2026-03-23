using DevHabit.Application.DTOs.Tags;
using DevHabit.Application.Results;

namespace DevHabit.Application.Services.Abstractions;

public interface ITagService
{
    Task<ServiceResult<TagDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ServiceResult<List<TagDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<TagDto>> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<TagDto>> UpdateAsync(string id, UpdateTagDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> ChangeStatusAsync(string id, ChangeStatusRequest request, CancellationToken cancellationToken = default);
}

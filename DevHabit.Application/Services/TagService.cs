using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Application.DTOs.Tags;
using DevHabit.Application.Exceptions;
using DevHabit.Application.Interfaces;
using DevHabit.Application.Results;
using DevHabit.Application.Services.Abstractions;
using DevHabit.Domain.Entities;
using TagMapping = DevHabit.Application.DTOs.Tags.TagMapping;

namespace DevHabit.Application.Services;

public sealed class TagService : BaseService<TagService>, ITagService
{
    public TagService(
        IUnitOfWork uow,
        RequestScopedService scopedService,
        IFileService fileService,
        ICacheService cache)
        : base(uow, scopedService, fileService, cache)
    {
    }

    public async Task<ServiceResult<TagDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.GetRepository<Tag>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException(nameof(Tag), id);
        }

        return ServiceResult<TagDto>.SuccessData(TagMapping.ToDto(entity));
    }

    public async Task<ServiceResult<List<TagDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var key = $"TagsList_{_scopedService.StoreId ?? "default"}";

        var result = await _cache.GetOrCreateAsync(key, async () =>
        {
            var query = _uow.GetRepository<Tag>().Query()
                .Where(t => t.RecordStatus == EntityStatus.Active)
                .OrderByDescending(t => t.CreatedAtUtc);

            var entities = await Task.Run(() => query.ToList(), cancellationToken);
            return entities;
        });

        var mapped = result!.Select(TagMapping.ToDto).ToList();
        return ServiceResult<List<TagDto>>.SuccessWithCount(mapped, mapped.Count);
    }

    public async Task<ServiceResult<TagDto>> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default)
    {
        var tag = dto.ToEntity();
        tag.CreatedBy = _scopedService.UserId;

        _uow.GetRepository<Tag>().Add(tag);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(Tag));

        return ServiceResult<TagDto>.SuccessData(TagMapping.ToDto(tag));
    }

    public async Task<ServiceResult<TagDto>> UpdateAsync(string id, UpdateTagDto dto, CancellationToken cancellationToken = default)
    {
        var tag = await _uow.GetRepository<Tag>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag == null)
        {
            throw new EntityNotFoundException(nameof(Tag), id);
        }

        tag.UpdateEntity(dto);
        tag.UpdatedBy = _scopedService.UserId;
        tag.UpdatedAtUtc = DateTime.UtcNow;

        _uow.GetRepository<Tag>().Update(tag);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(Tag));

        return ServiceResult<TagDto>.SuccessData(TagMapping.ToDto(tag));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync<Tag>(id);
    }

    public async Task<ServiceResult<string>> ChangeStatusAsync(string id, ChangeStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync<Tag>(id, request, cancellationToken);
    }
}

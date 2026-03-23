using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Application.DTOs.Habits;
using DevHabit.Application.Exceptions;
using DevHabit.Application.Interfaces;
using DevHabit.Application.Results;
using DevHabit.Application.Services.Abstractions;
using DevHabit.Domain.Entities;
using DevHabitMapping = DevHabit.Application.DTOs.Habits.HabitMapping;

namespace DevHabit.Application.Services;

public sealed class HabitService : BaseService<HabitService>, IHabitService
{
    public HabitService(
        IUnitOfWork uow,
        RequestScopedService scopedService,
        IFileService fileService,
        ICacheService cache)
        : base(uow, scopedService, fileService, cache)
    {
    }

    public async Task<ServiceResult<HabitDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.GetRepository<Habit>()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException(nameof(Habit), id);
        }

        return ServiceResult<HabitDto>.SuccessData(HabitMapping.ToDto(entity));
    }

    public async Task<ServiceResult<List<HabitDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var key = $"HabitsList_{_scopedService.StoreId ?? "default"}";

        var result = await _cache.GetOrCreateAsync(key, async () =>
        {
            var query = _uow.GetRepository<Habit>().Query()
                .Where(h => h.RecordStatus == EntityStatus.Active)
                .OrderByDescending(h => h.CreatedAtUtc);

            var entities = await Task.Run(() => query.ToList(), cancellationToken);
            return entities;
        });

        var mapped = result!.Select(HabitMapping.ToDto).ToList();
        return ServiceResult<List<HabitDto>>.SuccessWithCount(mapped, mapped.Count);
    }

    public async Task<ServiceResult<List<HabitDto>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = $"HabitsList_{userId}_{_scopedService.StoreId ?? "default"}";

        var result = await _cache.GetOrCreateAsync(key, async () =>
        {
            var query = _uow.GetRepository<Habit>().Query()
                .Where(h => h.CreatedBy == userId && h.RecordStatus == EntityStatus.Active)
                .OrderByDescending(h => h.CreatedAtUtc);

            var entities = await Task.Run(() => query.ToList(), cancellationToken);
            return entities;
        });

        var mapped = result!.Select(HabitMapping.ToDto).ToList();
        return ServiceResult<List<HabitDto>>.SuccessWithCount(mapped, mapped.Count);
    }

    public async Task<ServiceResult<HabitDto>> CreateAsync(CreateHabitDto dto, CancellationToken cancellationToken = default)
    {
        var habit = dto.ToEntity();
        habit.CreatedBy = _scopedService.UserId;

        _uow.GetRepository<Habit>().Add(habit);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(Habit));

        return ServiceResult<HabitDto>.SuccessData(HabitMapping.ToDto(habit));
    }

    public async Task<ServiceResult<HabitDto>> UpdateAsync(string id, UpdateHabitDto dto, CancellationToken cancellationToken = default)
    {
        var habit = await _uow.GetRepository<Habit>()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (habit == null)
        {
            throw new EntityNotFoundException(nameof(Habit), id);
        }

        habit.UpdateFromDto(dto);
        habit.UpdatedBy = _scopedService.UserId;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        _uow.GetRepository<Habit>().Update(habit);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(Habit));

        return ServiceResult<HabitDto>.SuccessData(HabitMapping.ToDto(habit));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DeleteAsync<Habit>(id);
    }

    public async Task<ServiceResult<string>> ChangeStatusAsync(string id, ChangeStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync<Habit>(id, request, cancellationToken);
    }

    public async Task<ServiceResult<HabitDtoWithTags>> GetWithTagsAsync(string id, CancellationToken cancellationToken = default)
    {
        var habit = await _uow.GetRepository<Habit>()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (habit == null)
        {
            throw new EntityNotFoundException(nameof(Habit), id);
        }

        var tags = habit.HabitTags.Select(ht => ht.TagId).ToList();

        var dtoWithTags = new HabitDtoWithTags(
            habit.Id,
            habit.Name,
            habit.Description,
            habit.Type,
            new FrequencyDto(habit.Frequency.FrequencyType, habit.Frequency.TimesPerPeriod),
            new TargetDto(habit.Targets.Value, habit.Targets.Units),
            habit.Status,
            habit.IsArchived,
            habit.EndDate,
            habit.Milestone == null ? null : new MilestoneDto(habit.Milestone.Target, habit.Milestone.Current),
            habit.CreatedAtUtc,
            habit.UpdatedAtUtc,
            habit.LastCompletedAtUtc,
            tags
        );

        return ServiceResult<HabitDtoWithTags>.SuccessData(dtoWithTags);
    }
}

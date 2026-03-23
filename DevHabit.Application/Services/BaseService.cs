using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevHabit.Application.Attributes;
using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Application.Exceptions;
using DevHabit.Application.Interfaces;
using DevHabit.Application.Results;
using DevHabit.Domain.Entities;

namespace DevHabit.Application.Services;

public abstract class BaseService<T> where T : class
{
    protected readonly IUnitOfWork _uow;
    protected readonly RequestScopedService _scopedService;
    protected readonly IFileService _fileService;
    protected readonly ICacheService _cache;

    protected BaseService(
        IUnitOfWork uow,
        RequestScopedService scopedService,
        IFileService fileService,
        ICacheService cache)
    {
        _uow = uow;
        _scopedService = scopedService;
        _fileService = fileService;
        _cache = cache;
    }

    protected async Task<ServiceResult<List<TModel>>> ListAsync<TEntity, TModel>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        string orderByDirection = "ASC",
        CancellationToken cancellationToken = default) where TEntity : BaseAuditEntity
    {
        var key = $"{typeof(TEntity).Name}List_{_scopedService.StoreId ?? "default"}";

        var result = await _cache.GetOrCreateAsync(key, async () =>
        {
            var query = _uow.GetRepository<TEntity>().Query();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderByDirection.ToUpper() == "DESC"
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }

            var entities = await Task.Run(() => query.ToList(), cancellationToken);
            return entities;
        });

        var mapped = MapToModelList<TEntity, TModel>(result!);
        return ServiceResult<List<TModel>>.SuccessWithCount(mapped, result!.Count);
    }

    protected async Task<ServiceResult<TModel>> GetAsync<TEntity, TModel>(
        string id,
        CancellationToken cancellationToken = default) where TEntity : BaseAuditEntity
    {
        var entity = await _uow.GetRepository<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, id);
        }

        var result = MapToModel<TEntity, TModel>(entity);
        return ServiceResult<TModel>.SuccessData(result);
    }

    public async Task<ServiceResult<bool>> DeleteAsync<TEntity>(string id) where TEntity : BaseAuditEntity
    {
        await ChangeStatusAsync<TEntity>(id, new ChangeStatusRequest
        {
            Status = (int)EntityStatus.Deleted,
            Notes = "[Deleted]"
        });

        await ClearEntityCacheAsync(typeof(TEntity));

        return ServiceResult<bool>.SuccessData(true, "Deleted successfully");
    }

    protected async Task<ServiceResult<TResult>> CreateAsync<TEntity, TModel, TResult>(
        TModel model,
        Dictionary<string, Func<TModel, bool>>? validationRules = null,
        CancellationToken cancellationToken = default) where TEntity : BaseAuditEntity
    {
        if (validationRules != null)
        {
            var invalidProperties = new Dictionary<string, object>();

            foreach (var rule in validationRules)
            {
                var propertyName = rule.Key;
                var validationFunc = rule.Value;

                var property = typeof(TModel).GetProperty(propertyName);
                if (property != null)
                {
                    var propertyValue = property.GetValue(model);

                    if (!validationFunc(model))
                    {
                        invalidProperties.Add(propertyName, propertyValue ?? "null");
                    }
                }
            }

            if (invalidProperties.Any())
            {
                throw new ValidationException("Validation failed", invalidProperties);
            }
        }

        await ProcessImagePropertiesAsync(model);

        var entity = MapToEntity<TModel, TEntity>(model);

        _uow.GetRepository<TEntity>().Add(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(TEntity));

        var result = MapToModel<TEntity, TResult>(entity);
        return ServiceResult<TResult>.SuccessData(result);
    }

    protected async Task<ServiceResult<TResult>> UpdateAsync<TEntity, TModel, TResult>(
        string id,
        TModel model,
        Dictionary<string, Func<TModel, bool>>? validationRules = null,
        CancellationToken cancellationToken = default) where TEntity : BaseAuditEntity
    {
        var entity = await _uow.GetRepository<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, id);
        }

        if (validationRules != null)
        {
            var invalidProperties = new Dictionary<string, object>();

            foreach (var rule in validationRules)
            {
                var propertyName = rule.Key;
                var validationFunc = rule.Value;

                var property = typeof(TModel).GetProperty(propertyName);
                if (property != null)
                {
                    var propertyValue = property.GetValue(model);

                    if (!validationFunc(model))
                    {
                        invalidProperties.Add(propertyName, propertyValue ?? "null");
                    }
                }
            }

            if (invalidProperties.Any())
            {
                throw new ValidationException("Validation failed", invalidProperties);
            }
        }

        var imageProperties = GetImagePropertyNames<TModel>();
        await ProcessImagePropertiesForUpdateAsync(model, entity, imageProperties);

        foreach (var property in typeof(TModel).GetProperties())
        {
            if (property.Name == "Id")
            {
                continue;
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                property.PropertyType != typeof(string))
            {
                continue;
            }

            if (imageProperties.Contains(property.Name))
            {
                continue;
            }

            var value = property.GetValue(model);

            var entityProperty = typeof(TEntity).GetProperty(property.Name);
            if (entityProperty != null && value != null)
            {
                entityProperty.SetValue(entity, value);
            }
        }

        _uow.GetRepository<TEntity>().Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(TEntity));

        return ServiceResult<TResult>.SuccessData(MapToModel<TEntity, TResult>(entity));
    }

    public async Task<ServiceResult<string>> ChangeStatusAsync<TEntity>(
        string id,
        ChangeStatusRequest request,
        CancellationToken cancellationToken = default) where TEntity : BaseAuditEntity
    {
        var entity = await _uow.GetRepository<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity).Name, id);
        }

        entity.RecordStatus = (EntityStatus)request.Status;
        entity.StatusNotes = request.Notes;

        _uow.GetRepository<TEntity>().Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        await ClearEntityCacheAsync(typeof(TEntity));

        return ServiceResult<string>.SuccessData(id, "Status changed successfully");
    }

    protected async Task ClearEntityCacheAsync(Type type)
    {
        await _cache.TryRemove(type.Name);
        await _cache.TryRemove($"{type.Name}List");
        await _cache.TryRemove($"{type.Name}List_{_scopedService.StoreId}");
    }

    protected virtual List<string> GetImagePropertyNames<TModel>()
    {
        return typeof(TModel)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<IsFileAttribute>() != null)
            .Select(p => p.Name)
            .ToList();
    }

    private async Task ProcessImagePropertiesAsync<TModel>(TModel model)
    {
        var imageProperties = GetImagePropertyNames<TModel>();
        if (!imageProperties.Any())
        {
            return;
        }

        foreach (var propertyName in imageProperties)
        {
            var property = typeof(TModel).GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(string))
            {
                var imageData = property.GetValue(model) as string;
                if (!string.IsNullOrEmpty(imageData))
                {
                    var imagePath = await UploadImageAsync(imageData);
                    property.SetValue(model, imagePath);
                }
            }
        }
    }

    private async Task ProcessImagePropertiesForUpdateAsync<TModel, TEntity>(
        TModel model,
        TEntity entity,
        IEnumerable<string> imageProperties) where TEntity : BaseAuditEntity
    {
        var propertiesList = imageProperties.ToList();
        if (propertiesList.Count == 0)
        {
            return;
        }

        foreach (var propertyName in propertiesList)
        {
            var modelProperty = typeof(TModel).GetProperty(propertyName);
            var entityProperty = typeof(TEntity).GetProperty(propertyName);

            if (modelProperty != null && entityProperty != null &&
                modelProperty.PropertyType == typeof(string) &&
                entityProperty.PropertyType == typeof(string))
            {
                var newImageData = modelProperty.GetValue(model) as string;
                var originalImage = entityProperty.GetValue(entity) as string;

                if (string.IsNullOrEmpty(newImageData))
                {
                    entityProperty.SetValue(entity, string.Empty);
                }
                else
                {
                    var imagePath = await UploadImageAsync(newImageData, originalImage);

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        entityProperty.SetValue(entity, imagePath);
                    }
                }
            }
        }
    }

    protected async Task<string> UploadImageAsync(string newImageData, string? defaultImage = null)
    {
        if (!string.IsNullOrEmpty(defaultImage) && !string.IsNullOrEmpty(newImageData) && newImageData.EndsWith(defaultImage, StringComparison.OrdinalIgnoreCase))
        {
            return defaultImage;
        }

        if (!string.IsNullOrEmpty(newImageData))
        {
            if (newImageData.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return newImageData;
            }

            return await _fileService.Upload(newImageData);
        }

        if (!string.IsNullOrEmpty(defaultImage))
        {
            return defaultImage;
        }

        return string.Empty;
    }

    protected virtual TEntity MapToEntity<TModel, TEntity>(TModel model) where TEntity : BaseAuditEntity
    {
        throw new NotImplementedException($"Mapping from {typeof(TModel).Name} to {typeof(TEntity).Name} must be implemented in derived service.");
    }

    protected virtual TModel MapToModel<TEntity, TModel>(TEntity entity) where TEntity : BaseAuditEntity
    {
        throw new NotImplementedException($"Mapping from {typeof(TEntity).Name} to {typeof(TModel).Name} must be implemented in derived service.");
    }

    protected virtual List<TModel> MapToModelList<TEntity, TModel>(List<TEntity> entities) where TEntity : BaseAuditEntity
    {
        throw new NotImplementedException($"Mapping from List<{typeof(TEntity).Name}> to List<{typeof(TModel).Name}> must be implemented in derived service.");
    }
}

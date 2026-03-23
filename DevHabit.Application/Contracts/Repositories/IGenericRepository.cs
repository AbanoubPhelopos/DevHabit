using System.Linq.Expressions;
using DevHabit.Domain.Entities;

namespace DevHabit.Application.Contracts.Repositories;

public interface IGenericRepository<TEntity> where TEntity : BaseAuditEntity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, object>>? orderBy = null, string? orderByDirection = "ASC", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, object>>? orderBy = null, string? orderByDirection = "ASC", int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, object>>? orderBy = null, string? orderByDirection = "ASC", int? skip = null, int? take = null, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
    void Attach(TEntity entity);
    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryWithOrdering(string orderByDirection = "ASC");
    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, string? orderBy = null, string? orderByDirection = "ASC");
}

using System.Linq.Expressions;
using DevHabit.Application.Contracts.Repositories;
using DevHabit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Infrastructure.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseAuditEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        string? orderByDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        return await GetAsync(predicate, orderBy, orderByDirection, null, null, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        string? orderByDirection = "ASC",
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderByDirection?.ToUpper() == "DESC"
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        string? orderByDirection = "ASC",
        int? skip = null,
        int? take = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderByDirection?.ToUpper() == "DESC"
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.AsNoTracking().CountAsync(cancellationToken);
        }

        return await _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AttachRange(entities);
        foreach (var entity in entities)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual void Attach(TEntity entity)
    {
        _dbSet.Attach(entity);
    }

    public virtual IQueryable<TEntity> Query()
    {
        return _dbSet.AsNoTracking();
    }

    public virtual IQueryable<TEntity> QueryWithOrdering(string orderByDirection = "ASC")
    {
        return Query();
    }

    public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, string? orderBy = null, string? orderByDirection = "ASC")
    {
        return Query().Where(predicate);
    }
}

using System.Collections.Concurrent;
using DevHabit.Application.Contracts.Repositories;
using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(DbContext context)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseAuditEntity
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        var repositoryInstance = new GenericRepository<TEntity>(_context);
        _repositories.TryAdd(type, repositoryInstance);

        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction has been started. Call BeginTransactionAsync first.");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);

        try
        {
            await action();
            await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action();
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

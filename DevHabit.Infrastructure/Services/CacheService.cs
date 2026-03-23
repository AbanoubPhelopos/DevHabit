using DevHabit.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Infrastructure.Services;

public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }

        var value = await factory();
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };

        _cache.Set(key, value, options);
        return value;
    }

    public Task TryRemove(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task TryRemoveByPrefix(string prefix)
    {
        return Task.CompletedTask;
    }
}

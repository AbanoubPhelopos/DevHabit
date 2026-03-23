namespace DevHabit.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task TryRemove(string key);
    Task TryRemoveByPrefix(string prefix);
}

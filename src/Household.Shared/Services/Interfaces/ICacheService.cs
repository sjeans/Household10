namespace Household.Shared.Services.Interfaces;

public interface ICacheService<T>
{
    Task<T> GetOrCreateAsync(string cacheKey, Func<Task<T>> fetchFromSourceAsync, TimeSpan? expiration = null, TimeSpan? slidingExpiration = null);

    Task<bool> ExpireAsync(string cacheKey);

    Task<bool> ExistsAsync(string cacheKey);
}

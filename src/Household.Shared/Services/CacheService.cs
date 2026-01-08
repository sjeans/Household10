using Household.Shared.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Household.Shared.Services;

public class CacheService<T> : ICacheService<T>
{
    private readonly IDistributedCache _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync(string cacheKey, Func<Task<T>> fetchFromSourceAsync, TimeSpan? expiration = null, TimeSpan? slidingExpiration = null)
    {
        expiration ??= TimeSpan.FromHours(3);

        string? json = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(json))
            return JsonSerializer.Deserialize<T>(json!, _jsonOptions)!;

        await _lock.WaitAsync();
        try
        {
            json = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(json))
                return JsonSerializer.Deserialize<T>(json!, _jsonOptions)!;

            T result = await fetchFromSourceAsync();

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result, _jsonOptions), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = slidingExpiration,
            });

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ExistsAsync(string cacheKey)
    {
        return await _cache.GetAsync(cacheKey) is not null;
    }

    public async Task<bool> ExpireAsync(string cacheKey)
    {
        // Check if the key exists
        bool existing = await ExistsAsync(cacheKey);
        if (!existing)
            return false;

        // Remove the key
        await _cache.RemoveAsync(cacheKey);

        return true;
    }
}

using Household.Shared.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedLockNet;
using RedLockNet.SERedis;
using ILogger = Serilog.ILogger;

namespace Household.Shared.Services;

public class DistributedCacheService<T>(IDistributedCache cache, RedLockFactory lockFactory, ILogger logger) : ICacheService<T>
{
    private readonly IDistributedCache _cache = cache;
    private readonly RedLockFactory _lockFactory = lockFactory;
    private readonly ILogger _logger = logger;

    public async Task<T> GetOrCreateAsync(string cacheKey, Func<Task<T>> fetchFromSourceAsync, TimeSpan? expiration = null, TimeSpan? slidingExpiration = null)
    {
        _logger.Information("Cache GET start | Key={Key} | Node={Node}", cacheKey, Environment.MachineName);

        // Fast path
        string? json = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            _logger.Information("Cache HIT | Key={Key}", cacheKey);
            return JsonConvert.DeserializeObject<T>(json)!;
        }

        // Distributed lock
        await using IRedLock redLock = await _lockFactory.CreateLockAsync(resource: $"lock:{cacheKey}", expiryTime: TimeSpan.FromMinutes(2), waitTime: TimeSpan.FromSeconds(10), retryTime: TimeSpan.FromSeconds(1)); // safer

        if (!redLock.IsAcquired)
        {
            _logger.Information("Lock NOT acquired | Key={Key} | Waiting for cache", cacheKey);

            // Wait briefly for the other instance to populate cache
            await Task.Delay(200);

            json = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _logger.Information("Cache HIT after wait | Key={Key}", cacheKey);
                return JsonConvert.DeserializeObject<T>(json)!;
            }

            // Last resort: fetch + cache anyway (idempotent write)
            _logger.Warning("Cache still empty after wait | Key={Key} | Fetching anyway", cacheKey);
        }

        // Double-check after lock
        json = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            _logger.Information("Cache HIT after lock | Key={Key}", cacheKey);
            return JsonConvert.DeserializeObject<T>(json)!;
        }

        // Fetch
        _logger.Information("Cache MISS | Fetching | Key={Key}", cacheKey);
        T? result = await fetchFromSourceAsync();

        // Always cache
        await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = slidingExpiration
            });

        _logger.Information("Cache SET | Key={Key} | Node={Node}", cacheKey, Environment.MachineName);

        return result;
    }

    public async Task<bool> ExistsAsync(string cacheKey)
    {
        byte[]? raw = await _cache.GetAsync(cacheKey);
        string? str = await _cache.GetStringAsync(cacheKey);

        _logger.Information("Raw exists: {Raw}, String exists: {Str}, Cache access: {Key} at {Time} from {Caller}", raw != null, str != null, cacheKey, DateTime.UtcNow, Environment.MachineName);

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
        _logger.Information("Cache EXPIRE | Key={Key} | Node={Node}", cacheKey, Environment.MachineName);

        return true;
    }
}

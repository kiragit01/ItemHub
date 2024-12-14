using ItemHub.Interfaces;
using ItemHub.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ItemHub.Repository;

public class RedisCacheRepository(
    IDistributedCache cache,
    ILogger<RedisCacheRepository> logger,
    CircuitBreaker circuitBreaker)
    : ICacheRepository
{
    public async Task SetAsync<T>(string key, T item, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
    {
        if (circuitBreaker.IsOpen)
        {
            logger.LogWarning("Redis is disabled. All requests go to the server. Unavailable \"{Key}\"", key);
            return;
        }

        try
        {
            var options = new DistributedCacheEntryOptions();
            if (absoluteExpireTime.HasValue)
                options.SetAbsoluteExpiration(absoluteExpireTime.Value);
            if (slidingExpireTime.HasValue)
                options.SetSlidingExpiration(slidingExpireTime.Value);

            var jsonData = JsonConvert.SerializeObject(item);
            await cache.SetStringAsync(key, jsonData, options);
            circuitBreaker.Reset();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при установке значения в Redis по ключу \"{Key}\"", key);
            circuitBreaker.RecordFailure();
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (circuitBreaker.IsOpen)
        {
            logger.LogWarning("Redis is disabled. All requests go to the server. Unavailable \"{Key}\"", key);
            return default;
        }

        try
        {
            var jsonData = await cache.GetStringAsync(key);
            circuitBreaker.Reset();
            return jsonData == null
                ? default
                : JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении значения из Redis по ключу \"{Key}\"", key);
            circuitBreaker.RecordFailure();
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (circuitBreaker.IsOpen)
        {
            logger.LogWarning("Redis is disabled. All requests go to the server. Unavailable \"{Key}\"", key);
            return;
        }

        try
        {
            await cache.RemoveAsync(key);
            circuitBreaker.Reset();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении значения из Redis по ключу {Key}", key);
            circuitBreaker.RecordFailure();
        }
    }
}

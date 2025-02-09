using ItemHub.HealthChecks;
using ItemHub.Interfaces;
using ItemHub.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ItemHub.Repository;

public class RedisCacheRepository(
    IDistributedCache cache,
    ILogger<RedisCacheRepository> logger,
    RedisHealthCheck healthCheck)
    : ICacheRepository
{
    public async Task SetAsync<T>(string key, T item, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
    {
        if (!healthCheck.TryCheck()) return;

        try
        {
            var options = new DistributedCacheEntryOptions();
            if (absoluteExpireTime.HasValue)
                options.SetAbsoluteExpiration(absoluteExpireTime.Value);
            if (slidingExpireTime.HasValue)
                options.SetSlidingExpiration(slidingExpireTime.Value);

            var jsonData = JsonConvert.SerializeObject(item);
            await cache.SetStringAsync(key, jsonData, options);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при установке значения в Redis по ключу \"{Key}\"", key);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (!healthCheck.TryCheck()) return default;

        try
        {
            var jsonData = await cache.GetStringAsync(key);
            return jsonData == null
                ? default
                : JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении значения из Redis по ключу \"{Key}\"", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!healthCheck.TryCheck()) return;
        
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении значения из Redis по ключу {Key}", key);
        }
    }
}

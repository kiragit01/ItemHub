using ItemHub.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ItemHub.Repository;

public class RedisCacheRepository(IDistributedCache cache) : ICacheRepository
{
    public async Task SetAsync<T>(string key, T item, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions();
        if (absoluteExpireTime.HasValue)
            options.SetAbsoluteExpiration(absoluteExpireTime.Value);
        if (slidingExpireTime.HasValue)
            options.SetSlidingExpiration(slidingExpireTime.Value);

        var jsonData = JsonConvert.SerializeObject(item);
        await cache.SetStringAsync(key, jsonData, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var jsonData = await cache.GetStringAsync(key);
            return jsonData == null 
                ? default 
                : JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
}
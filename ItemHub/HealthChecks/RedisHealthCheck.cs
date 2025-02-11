using ItemHub.Interfaces;
using ItemHub.Utilities;
using Microsoft.Extensions.Caching.Distributed;

namespace ItemHub.HealthChecks;

public class RedisHealthCheck(
    IDistributedCache cache, 
    ILogger<RedisHealthCheck> logger) 
    : IServiceHealthCheck
{
    private static readonly object _pingLock = new();
    private static DateTime _lastPingTime = DateTime.MinValue;
    private static bool _redisOk = false;
    
    /// <summary>
    /// Возвращает текущее состояние Redis. Если прошло достаточно времени с момента последней проверки,
    /// инициируется асинхронный пинг.
    /// </summary>
    public bool TryCheck()
    {
        bool needPing = false;
        lock (_pingLock)
        {
            if (DateTime.UtcNow - _lastPingTime >= Constants.ServiceHealthCheckTimeSpan)
            {
                _lastPingTime = DateTime.UtcNow;
                needPing = true;
            }
        }
        if (needPing)
        {
            _ = PingRedisAsync();
        }
        return _redisOk;
    }

    /// <summary>
    /// Асинхронно проверяет работоспособность Redis: пытается установить и получить тестовое значение.
    /// </summary>
    private Task PingRedisAsync()
    {
        return Task.Run(async () =>
        {
            try
            {
                const string testKey = "RedisHealthCheckKey";
                const string testValue = "OK";

                // Устанавливаем тестовое значение с коротким временем жизни
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                };

                await cache.SetStringAsync(testKey, testValue, options);
                var value = await cache.GetStringAsync(testKey);

                bool isHealthy = value == testValue;
                lock (_pingLock)
                {
                    _redisOk = isHealthy;
                }
                if (!isHealthy) logger.LogInformation("Redis is disabled. All requests go to the server.");
            }
            catch (Exception ex)
            {
                lock (_pingLock)
                {
                    _redisOk = false;
                }
                logger.LogWarning("Redis health check error.");
            }
        });
    }
}
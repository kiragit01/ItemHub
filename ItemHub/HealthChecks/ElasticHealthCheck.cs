using ItemHub.Interfaces;
using ItemHub.Utilities;
using Nest;

namespace ItemHub.HealthChecks;

public class ElasticHealthCheck(
    IElasticClient elasticClient, 
    ILogger<ElasticHealthCheck> logger)
    : IServiceHealthCheck
{
    private static readonly object _pingLock = new();
    private static DateTime _lastPingTime = DateTime.MinValue;
    private static bool _elasticOk = false;

    /// <summary>
    /// Возвращает текущее состояние Elasticsearch. При необходимости инициирует асинхронную проверку.
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
            _ = PingElasticAsync();
        }
        return _elasticOk;
    }

    /// <summary>
    /// Асинхронно проверяет состояние Elasticsearch посредством Ping.
    /// </summary>
    private Task PingElasticAsync()
    {
        return Task.Run(async () =>
        {
            try
            {
                var pingResponse = await elasticClient.PingAsync();
                bool isHealthy = pingResponse.IsValid;
                lock (_pingLock)
                {
                    _elasticOk = isHealthy;
                }
                if (!isHealthy) logger.LogInformation("Elasticsearch is disabled. All requests go to the server.");
            }
            catch (Exception ex)
            {
                lock (_pingLock)
                {
                    _elasticOk = false;
                }
                logger.LogError(ex, "Elasticsearch health check error.");
            }
        });
    }
}
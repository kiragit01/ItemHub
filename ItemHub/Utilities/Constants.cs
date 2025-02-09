namespace ItemHub.Utilities;

public static class Constants
{
    //Время жизни кэша
    public static TimeSpan UserCacheSlidingExpiration { get; } = TimeSpan.FromDays(1);
    public static TimeSpan ItemCacheSlidingExpiration { get; } = TimeSpan.FromMinutes(5);
    public static TimeSpan IndexCacheAbsoluteExpiration { get; } = TimeSpan.FromSeconds(30);
    public static TimeSpan ItemViewsCacheSlidingExpiration { get; } = TimeSpan.FromHours(1);
    //Время проверки работоспособности сервисов;
    public static TimeSpan ServiceHealthCheckTimeSpan { get; } = TimeSpan.FromSeconds(30);
    
}
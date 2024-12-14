namespace ItemHub.Interfaces;

public interface ICacheRepository
{
    Task SetAsync<T>(string key, T item, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}
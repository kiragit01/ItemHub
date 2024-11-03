namespace ItemHub.Repository.Interfaces;

public interface IRepository
{
    public Task<T?> Get<T, K>(K key);
    public Task Add<T>(T entity);
    public Task Update<T>(T entity);
    public Task Remove<T>(T entity);
}
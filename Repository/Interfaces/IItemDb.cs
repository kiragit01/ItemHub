using ItemHub.Models.OnlyItem;

namespace ItemHub.Repository.Interfaces;

public interface IItemDb
{
    public IQueryable<Item> AllItems();
    public Task<Item?> GetItemNoTracking(Guid id);
    public Task<Item?> GetItem(Guid id);
    public Task AddItem(Item item);
    public Task UpdateItem(Item item);
    public Task RemoveItem(Item item);
}
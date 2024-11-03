using ItemHub.Models.OnlyItem;

namespace ItemHub.Interfaces;

public interface IItemRepository
{
    public IQueryable<Item> AllItems();
    public Task<Item?> GetItemNoTrackingAsync(Guid id);
    public Task<Item?> GetItemAsync(Guid id);
    public Task<bool> ItemExistsAsync(Guid id);
    public Task AddItemAsync(Item item);
    public Task UpdateItemAsync(Item item);
    public Task RemoveItemAsync(Item item);
}
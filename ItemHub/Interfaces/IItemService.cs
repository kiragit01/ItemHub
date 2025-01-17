using ItemHub.Models.OnlyItem;

namespace ItemHub.Interfaces;

public interface IItemService
{
    public Task<Item?> GetItemNoTracking(Guid id);
    public Task<string?> CreateAsync(ItemModel model);
    public Task<string?> UpdateAsync(Guid id, ItemModel model, List<string> savedImages);
    public Task<string?> DeleteAsync(Guid id);
    public Task RegisterViewAsync(Guid itemId);
}
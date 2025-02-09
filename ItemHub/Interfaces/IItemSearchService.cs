using ItemHub.Models.OnlyItem;

namespace ItemHub.Interfaces;

public interface IItemSearchService
{
    Task<List<Item>> SearchItemsAsync(string query, int? minPrice, int? maxPrice, bool onlyMine = false, bool favorite = false);
    Task<int> GetMaxPriceAsync();
    Task IndexItemAsync(Item item);
    Task DeleteItemAsync(Guid id);
    Task IndexAllAsync();
}
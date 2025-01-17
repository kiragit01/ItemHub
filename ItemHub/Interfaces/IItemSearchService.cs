using ItemHub.Models.OnlyItem;

namespace ItemHub.Interfaces;

public interface IItemSearchService
{
    Task<List<Item>> SearchItemsAsync(string query, int? minPrice, int? maxPrice);
    Task<int> GetMaxPriceAsync();
    Task IndexItemAsync(Item item);
    Task DeleteItemAsync(Guid id);
    Task IndexAllAsync();
}
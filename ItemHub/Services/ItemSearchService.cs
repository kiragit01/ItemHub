using ItemHub.HealthChecks;
using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;

namespace ItemHub.Services;

public class ItemSearchService( 
    IItemRepository itemRepository,
    IUserRepository userRepository,
    ElasticSearch elasticSearch,
    ElasticHealthCheck elasticHealthCheck,
    IUserContext userContext) 
    : IItemSearchService
{
    public async Task<int> GetMaxPriceAsync()
    {
        if (elasticHealthCheck.TryCheck()) 
            return await elasticSearch.GetMaxPriceAsync();
        return await itemRepository.GetMaxPriceAsync();
    }

    public async Task<List<Item>> SearchItemsAsync(
        string query, int? minPrice, int? maxPrice, bool onlyMine = false, bool favorite = false)
    {
        if (elasticHealthCheck.TryCheck())
        {
            try
            {
                var allFound = await elasticSearch.SearchItemsAsync(query, minPrice, maxPrice, onlyMine, favorite);
                var result = new List<Item>();
                if (favorite)
                {
                    var user = await userRepository.GetUserAsync();
                    if (user != null)
                    {
                        result = allFound.Where(x => x.Published && user.FavoritedItemsId.Contains(x.Id)).ToList();
                    }
                }
                else if (onlyMine)
                {
                    result = allFound.Where(x => x.Creator == userContext.Login).ToList();
                }
                else
                {
                    result = allFound.Where(x => x.Published).ToList();
                }
                return result;
            }
            catch
            {
                // Fallback на БД
            }
        }
        // Fallback — поиск средствами БД
        var itemsFromDb = itemRepository.AllItems().AsQueryable();
        itemsFromDb = onlyMine 
            ? itemsFromDb.Where(x => x.Creator == userContext.Login) 
            : itemsFromDb.Where(x => x.Published);

        // Псевдо-поиск:
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowered = query.ToLower();
            itemsFromDb = itemsFromDb
                .Where(x => x.Title.ToLower().Contains(lowered)
                            || x.Description!.ToLower().Contains(lowered));
        }
        if (minPrice.HasValue) itemsFromDb = itemsFromDb.Where(x => x.Price >= minPrice.Value);
        if (maxPrice.HasValue) itemsFromDb = itemsFromDb.Where(x => x.Price <= maxPrice.Value);

        var dbFound = itemsFromDb.ToList();
        return dbFound;
    }

    public async Task IndexItemAsync(Item item)
    {
        if(elasticHealthCheck.TryCheck()) await elasticSearch.IndexItemAsync(item);
    }

    public async Task DeleteItemAsync(Guid id)
    {
        if(elasticHealthCheck.TryCheck()) await elasticSearch.DeleteItemAsync(id);
    }

    public async Task IndexAllAsync()
    {
        if(elasticHealthCheck.TryCheck()) await elasticSearch.IndexAllAsync();
    }
}
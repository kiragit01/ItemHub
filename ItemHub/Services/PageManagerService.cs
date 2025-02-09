using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.Pages;
using ItemHub.Models.User;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class PageManagerService(
    IItemRepository itemRepository,
    IUserRepository userRepository,
    ICacheRepository cacheRepository,
    IItemSearchService searchService,
    IUserContext userContext) 
    : IPageManagerService
{
    private const int PageSize = 2; // количество элементов на странице
    
    public async Task<int> MaxPrice() => await searchService.GetMaxPriceAsync();

    public async Task<IndexViewModel> SearchItem(string query, int? minPrice, int? maxPrice, 
        int? page, bool onlyMine = false, bool favorite = false)
    {
        var key = $"searchItem-{query}-{minPrice}-{maxPrice}-{onlyMine}-{favorite}";
        var cache = await cacheRepository.GetAsync<List<Item>>(key);
        if (cache != null) return ViewModelForList(cache, page);
        var result = await searchService.SearchItemsAsync(query, minPrice, maxPrice, onlyMine, favorite);
        await cacheRepository.SetAsync(key, result, absoluteExpireTime: Constants.IndexCacheAbsoluteExpiration);
        return ViewModelForList(result, page);
    }
        
    public async Task<IndexViewModel?> MyItems(int? page)
    {
        var cache = await cacheRepository.GetAsync<User>(userContext.Login);
        if (cache != null) return ViewModelForList(cache.CustomItems, page);
        var user = await userRepository.GetUserAsync();
        await cacheRepository.SetAsync(userContext.Login, user, Constants.UserCacheSlidingExpiration);
        return user != null 
            ? ViewModelForList(user.CustomItems, page) 
            : null;
    }
        
    public async Task<IndexViewModel?> FavoritedItems(int? page)
    {
        var user = await cacheRepository.GetAsync<User>(userContext.Login);
        if (user == null)
        {
            user = await userRepository.GetUserAsync();
            if (user == null) return null;
            await cacheRepository.SetAsync(userContext.Login, user, Constants.UserCacheSlidingExpiration);
        }
        var favoritedItems = new List<Item>();
        var validItemIds = new List<Guid>();
        foreach (var id in user.FavoritedItemsId)
        {
            var item = await cacheRepository.GetAsync<Item>(id.ToString()) 
                       ?? await itemRepository.GetItemNoTrackingAsync(id);
            if (item == null) continue;
            favoritedItems.Add(item);
            validItemIds.Add(id);
        }
        user.FavoritedItemsId = validItemIds;
        var items = favoritedItems.Where(item => item.Published).ToList();
        await userRepository.UpdateUserAsync(user);
        return ViewModelForList(items, page);
    }
    
    private static IndexViewModel ViewModelForList(List<Item> allItems, int? page)
    {
        var currentPage = page ?? 1;
        var totalPages = (int)Math.Ceiling((double)allItems.Count / PageSize);
        if (currentPage < 1) currentPage = 1;
        if (currentPage > totalPages && totalPages > 0) currentPage = totalPages;
        var skip = (currentPage - 1) * PageSize;
        var items = allItems.Skip(skip).Take(PageSize).ToList();
        var pageViewModel = new PageViewModel(allItems.Count, currentPage, PageSize);
        return new IndexViewModel(items, pageViewModel);
    }
}
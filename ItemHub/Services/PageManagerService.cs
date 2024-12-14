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
    IUserContext userContext) 
    : IPageManagerService
{
    private const int PageSize = 2; // количество элементов на странице
    private const int MainPage = 1; // главная страница
    
    public async Task<IndexViewModel> Index(int? page)
    {
        var cache = await cacheRepository.GetAsync<List<Item>>("index");
        if (cache != null) return ViewModelForList(cache, page);
        var source = itemRepository.AllItems().Where(item => item.Published).ToList();
        await cacheRepository.SetAsync("index", source, absoluteExpireTime: Constants.IndexCacheAbsoluteExpiration);
        return ViewModelForList(source, page);
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
        var mainPage = page ?? MainPage;
        var items = allItems.Skip((mainPage - 1) * PageSize).Take(PageSize).ToList();
        var pageViewModel = new PageViewModel(allItems.Count, mainPage, PageSize);
        return new IndexViewModel(items, pageViewModel);
    }
}
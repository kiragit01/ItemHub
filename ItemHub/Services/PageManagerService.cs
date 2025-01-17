using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.Pages;
using ItemHub.Models.User;
using ItemHub.Utilities;
using Nest;

namespace ItemHub.Services;

public class PageManagerService(
    IItemRepository itemRepository,
    IUserRepository userRepository,
    ICacheRepository cacheRepository,
    IItemSearchService searchService,
    IElasticClient elasticClient, 
    IUserContext userContext) 
    : IPageManagerService
{
    private const int PageSize = 2; // количество элементов на странице
    private static readonly object _pingLock = new();
    private static DateTime _lastPingTime = DateTime.MinValue;
    private static bool _elasticOk = false; // по умолчанию считаем, что ES не поднят
    
    private async Task<bool> TryCheckElasticAsync()
    {
        const int PING_INTERVAL_SECONDS = 30;
        bool needPing;
        lock (_pingLock)
        {
            // Проверяем, не было ли последней проверки менее 30 секунд назад
            if (DateTime.UtcNow - _lastPingTime < TimeSpan.FromSeconds(PING_INTERVAL_SECONDS))
            {
                // Возвращаем текущее состояние без новой попытки
                return _elasticOk;
            }
            // Пора пинговать
            _lastPingTime = DateTime.UtcNow;
            needPing = true;
        }

        if (!needPing) return _elasticOk;
        try
        {
            var ping = await elasticClient.PingAsync(); 
            bool isValid = ping.IsValid; // true, если ES отвечает
            if (isValid) await searchService.IndexAllAsync();
            lock (_pingLock)
            {
                _elasticOk = isValid;
            }
        }
        catch
        {
            lock (_pingLock)
            {
                _elasticOk = false;
            }
        }

        return _elasticOk;
    }
    
    public async Task<int> MaxPrice() => await searchService.GetMaxPriceAsync();

    public async Task<IndexViewModel> SearchItem(string query, int? minPrice, int? maxPrice, int? page, bool onlyMine = false)
    {
        var cache = await cacheRepository.GetAsync<List<Item>>($"searchItem-{query}-{minPrice}-{maxPrice}-{onlyMine}");
        if (cache != null) return ViewModelForList(cache, page);
        bool esAvailable = await TryCheckElasticAsync();
        if (esAvailable)
        {
            try
            {
                var allFound = await searchService.SearchItemsAsync(query, minPrice, maxPrice);
                var published = onlyMine 
                    ? allFound.Where(x => x.Creator == userContext.Login).ToList() 
                    : allFound.Where(x => x.Published).ToList();
                await cacheRepository.SetAsync($"searchItem-{query}-{minPrice}-{maxPrice}-{onlyMine}", published, absoluteExpireTime: Constants.IndexCacheAbsoluteExpiration);
                return ViewModelForList(published, page);
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
        
        await cacheRepository.SetAsync($"searchItem-{query}-{minPrice}-{maxPrice}-{onlyMine}", dbFound, absoluteExpireTime: Constants.IndexCacheAbsoluteExpiration);
        return ViewModelForList(dbFound, page);
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
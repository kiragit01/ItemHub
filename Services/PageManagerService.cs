using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.Pages;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Services;

public class PageManagerService(
    IItemRepository itemRepository,
    IUserRepository userRepository) 
    : IPageManagerService
{
    private const int PageSize = 2; // количество элементов на странице
    private const int MainPage = 1; // главная страница
    
    
    public async Task<IndexViewModel> Index(int? page)
    {
        var source = itemRepository.AllItems();
        return await ViewModelForIQueryable(source, page);
    }
        
    public async Task<IndexViewModel?> MyItems(int? page)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return null;
        var userItems = user.CustomItems;
        return ViewModelForList(userItems, page);
    }
        
    public async Task<IndexViewModel?> FavoritedItems(int? page)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return null;
        var favoritedItems = new List<Item>();
        var validItemIds = new List<Guid>();
        foreach (var id in user.FavoritedItemsId)
        {
            var item = await itemRepository.GetItemNoTrackingAsync(id);
            if (item == null) continue;
            favoritedItems.Add(item);
            validItemIds.Add(id);
        }
        user.FavoritedItemsId = validItemIds;
        await userRepository.UpdateUserAsync(user);
        return ViewModelForList(favoritedItems, page);
    }
    
    private static IndexViewModel ViewModelForList(List<Item> allItems, int? page)
    {
        var mainPage = page ?? MainPage;
        var items = allItems.Skip((mainPage - 1) * PageSize).Take(PageSize).ToList();
        var pageViewModel = new PageViewModel(allItems.Count, mainPage, PageSize);
        return new IndexViewModel(items, pageViewModel);
    }
    private static async Task<IndexViewModel> ViewModelForIQueryable(IQueryable<Item> allItems, int? page)
    {
        var mainPage = page ?? MainPage;
        var count = allItems.Count();
        var items = await allItems.Skip((mainPage - 1) * PageSize).Take(PageSize).ToListAsync();
        var pageViewModel = new PageViewModel(count, mainPage, PageSize);
        return new IndexViewModel(items, pageViewModel);
    }
}
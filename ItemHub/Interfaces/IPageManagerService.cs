using ItemHub.Models.Pages;

namespace ItemHub.Interfaces;

public interface IPageManagerService
{
    public Task<int> MaxPrice();
    public Task<IndexViewModel> SearchItem(string query, int? minPrice, int? maxPrice, int? page, bool onlyMine);
    public Task<IndexViewModel?> MyItems(int? page);
    public Task<IndexViewModel?> FavoritedItems(int? page);
}
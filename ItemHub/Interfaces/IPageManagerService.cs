using ItemHub.Models.Pages;

namespace ItemHub.Interfaces;

public interface IPageManagerService
{
    public Task<IndexViewModel> Index(int? page);
    public Task<IndexViewModel?> MyItems(int? page);
    public Task<IndexViewModel?> FavoritedItems(int? page);
}
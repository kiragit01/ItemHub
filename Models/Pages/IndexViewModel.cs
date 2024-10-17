using ItemHub.Models.OnlyItem;

namespace ItemHub.Models.Pages;

public class IndexViewModel(IEnumerable<Item> items, PageViewModel pageViewModel)
{
    public IEnumerable<Item> Items { get; set; } = items;
    public PageViewModel PageViewModel { get; set; } = pageViewModel;
}
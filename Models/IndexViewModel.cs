using ItemHub.Models.OnlyItem;

namespace ItemHub.Models;

public class IndexViewModel
{
    public IndexViewModel(IEnumerable<Item> items, PageViewModel pageViewModel)
    {
        Items = items;
        PageViewModel = pageViewModel;
    }

    public IEnumerable<Item> Items { get; set; }
    public PageViewModel PageViewModel { get; set; }
}
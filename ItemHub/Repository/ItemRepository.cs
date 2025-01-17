using ItemHub.Data;
using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Repository;

public class ItemRepository(DataBaseContext db) : IItemRepository
{
    public IQueryable<Item> AllItems() => db.Items;
    public async Task RenameItemsUserAsync(List<Item> items, string newLogin)
    {
        var itemIds = items.Select(item => item.Id).ToList();
        await db.Items
            .Where(item => itemIds.Contains(item.Id))
            .ExecuteUpdateAsync(setters 
                => setters.SetProperty(item => item.Creator, newLogin));
    }

    public async Task<bool> AddViewItemAsync(Guid id)
    {
        var item = await GetItemAsync(id);
        if (item == null) return false;
        item.Views++;
        await db.SaveChangesAsync();
        return true;
    }
    
    public async Task<Item?> GetItemNoTrackingAsync(Guid id) => 
        await db.Items.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Item?> GetItemAsync(Guid id) => 
        await db.Items.FirstOrDefaultAsync(o => o.Id == id);
    
    public async Task<bool> ItemExistsAsync(Guid id) => 
        await db.Items.FirstOrDefaultAsync(o => o.Id == id) != null;
    
    public async Task AddItemAsync(Item item)
    {
        await db.Items.AddAsync(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
    }
    public async Task RemoveItemsUserAsync(List<Item> items)
    {
        var itemIds = items.Select(item => item.Id).ToList();
        await db.Items
            .Where(item => itemIds.Contains(item.Id))
            .ExecuteDeleteAsync();
    }
}
using ItemHub.Data;
using ItemHub.Models.OnlyItem;
using ItemHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Repository;

public class ItemRepository(DataBaseContext db) : IItemRepository
{
    public IQueryable<Item> AllItems() => db.Items;
    
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
}
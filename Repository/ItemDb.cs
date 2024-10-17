using ItemHub.Data;
using ItemHub.Models.OnlyItem;
using ItemHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Repository;

public class ItemDb(DataBaseContext db) : IItemDb
{
    public IQueryable<Item> AllItems() => db.Items;
    
    public async Task<Item?> GetItemNoTracking(Guid id) => 
        await db.Items.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Item?> GetItem(Guid id) => 
        await db.Items.FirstOrDefaultAsync(o => o.Id == id);
    public async Task AddItem(Item item)
    {
        await db.Items.AddAsync(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateItem(Item item)
    {
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task RemoveItem(Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
    }
}
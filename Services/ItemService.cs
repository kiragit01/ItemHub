using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Services;

public class ItemService(
    IUserRepository userRepository, 
    IItemRepository itemRepository, 
    IUserContext userContext)
    : IItemService
{

    public async Task<Item?> GetItemNoTracking(Guid id) => await itemRepository.GetItemNoTrackingAsync(id);
    
    public async Task<string?> CreateAsync(ItemModel model)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return "Вы не вошли в аккаунт";
        var id = Guid.NewGuid();
        var pathImages = await UploadFiles.UploadItemImages(model.Images, user.Login, id);
        Item item = new(id, model.Title, model.Description, user.Login, pathImages, model.Price, !model.Published);
        user.CustomItems.Add(item);
        try
        {
            await itemRepository.AddItemAsync(item);
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Item не сохраняется в таблице из-за чего не может быть связан с пользователем";
        }
        return null;
    }
    
    public async Task<string?> UpdateAsync(Guid id, ItemModel model, List<string> savedImages)
    {
        var itemDb = await itemRepository.GetItemAsync(id);
        if (itemDb == null) return "Такого товара не существует :(";
        var pathImages = await UploadFiles.UploadItemImages(model.Images, itemDb.Creator, id);
        pathImages.AddRange(savedImages);
        if(pathImages.Count != 1 && pathImages[0] == "images/NoImage.png") pathImages.RemoveAt(0);
            
        itemDb.PathImages = pathImages;
        itemDb.Title = model.Title;
        itemDb.Description = model.Description;
        itemDb.Price = model.Price;
        itemDb.UpdatedDate = DateTime.UtcNow;
            
        await itemRepository.UpdateItemAsync(itemDb);
        return null;
    }
    
    public async Task<string?> DeleteAsync(Guid id)
    {
        var item = await itemRepository.GetItemAsync(id);
        if (item == null) return "Такого товара не существует :(";
        if (userContext.Login != item.Creator) return null;
        await itemRepository.RemoveItemAsync(item);
        return null;
    }
}
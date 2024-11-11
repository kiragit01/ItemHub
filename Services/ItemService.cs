using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
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
        var pathImages = await UploadFiles.UploadItemImagesAsync(model.Images, user.Login, id);
        Item item = new(id, model.Title, model.Description, user.Login, pathImages, model.Price, !model.Published);
        user.CustomItems.Add(item);
        try
        {
            await itemRepository.AddItemAsync(item);
        }
        catch (DbUpdateConcurrencyException)
        {
            return $"Ошибка при создание \"{model.Title}\"";
        }
        return null;
    }
    
    public async Task<string?> UpdateAsync(Guid id, ItemModel model, List<string> savedImages)
    {
        var item = await itemRepository.GetItemAsync(id);
        if (item == null) return "Такого товара не существует :(";
        var pathImages = await UploadFiles.UploadItemImagesAsync(model.Images, item.Creator, id);
        pathImages.AddRange(savedImages);
        if(pathImages.Count != 1 && pathImages[0] == "images/NoImage.png") pathImages.RemoveAt(0);
        
        item.PathImages = pathImages;
        item.Title = model.Title;
        item.Description = model.Description;
        item.Price = model.Price;
        item.UpdatedDate = DateTime.UtcNow;
        
        await itemRepository.UpdateItemAsync(item);
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
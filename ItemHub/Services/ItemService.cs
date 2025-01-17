using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Services;

public class ItemService(
    IUserRepository userRepository, 
    IItemRepository itemRepository,
    ICacheRepository cacheRepository,
    IItemSearchService searchService,
    IMyCookieManager cookieManager,
    IUserContext userContext)
    : IItemService
{

    public async Task<Item?> GetItemNoTracking(Guid id)
    {
        var key = $"Item_{id}";
        var item = await cacheRepository.GetAsync<Item>(key);
        if (item != null) return item;
        item = await itemRepository.GetItemNoTrackingAsync(id);
        if (item != null) 
            await cacheRepository.SetAsync(key, item, Constants.ItemCacheSlidingExpiration);
        return item;
    }
    
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
            await cacheRepository.RemoveAsync("index");
            await cacheRepository.RemoveAsync(userContext.Login);
            await searchService.IndexItemAsync(item);
        }
        catch (DbUpdateConcurrencyException)
        {
            return $"Ошибка при создании \"{model.Title}\"";
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
        
        item.Update(model.Title, model.Description, pathImages, model.Price, !model.Published); //TODO Реализовать published в View

        await cacheRepository.RemoveAsync("index");
        await cacheRepository.RemoveAsync(userContext.Login);
        await itemRepository.UpdateItemAsync(item);
        await searchService.IndexItemAsync(item);
        await cacheRepository.SetAsync($"Item_{item.Id}", item, Constants.ItemCacheSlidingExpiration);
        return null;
    }
    
    public async Task<string?> DeleteAsync(Guid id)
    {
        var item = await itemRepository.GetItemAsync(id);
        if (item == null) return "Такого товара не существует :(";
        if (userContext.Login != item.Creator) return null;
        await itemRepository.RemoveItemAsync(item);
        await cacheRepository.RemoveAsync($"Item_{item.Id}");
        await cacheRepository.RemoveAsync("index");
        await cacheRepository.RemoveAsync(userContext.Login);
        await searchService.DeleteItemAsync(id);
        return null;
    }

    public async Task RegisterViewAsync(Guid itemId)
    {
        var userIdentifier = cookieManager.GetOrCreateUniqueViewId();
        var cacheKey = $"View_{userIdentifier}_{itemId}";
        var hasViewed = await cacheRepository.GetAsync<bool>(cacheKey);
        if (hasViewed)
        {
            var addedView = await itemRepository.AddViewItemAsync(itemId);
            if(!addedView) return;
            await cacheRepository.SetAsync(cacheKey, true, Constants.ItemCacheSlidingExpiration);
        }
        else
        {
            var viewedTime = cookieManager.GetLastViewedTimeFromCookie(itemId);
            if (viewedTime == null || (DateTime.UtcNow - viewedTime.Value) > Constants.ItemCacheSlidingExpiration)
            {
                var addedView = await itemRepository.AddViewItemAsync(itemId);
                if(!addedView) return;
            }
        }
        cookieManager.UpdateViewTimeInCookie(itemId);
    }
}
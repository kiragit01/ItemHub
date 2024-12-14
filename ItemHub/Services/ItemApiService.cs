using ItemHub.Interfaces;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class ItemApiService(
    IItemRepository itemRepository,
    ICacheRepository cacheRepository,
    IUserContext userContext)
    : IItemApiService
{
    public async Task<Result<bool>> TogglePublishItemAsync(Guid itemId)
    {
        var item = await itemRepository.GetItemAsync(itemId);
        if (item == null)
            return Result<bool>.Fail("Товар не найден.");

        if (item.Creator != userContext.Login)
            return Result<bool>.Fail("У вас нет прав для изменения этого товара.");

        item.Published = !item.Published;
        await itemRepository.UpdateItemAsync(item);
        await cacheRepository.RemoveAsync("index");
        await cacheRepository.RemoveAsync(userContext.Login);
        return Result<bool>.Ok(item.Published);
    }

    public bool DeleteImage(Guid itemId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var login = userContext.Login;
        var imagesPath = Path.Combine(WebRootPath.Path, "images", login, itemId.ToString());
        var filePath = Path.Combine(imagesPath, fileName);

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    public List<string?> GetSavedImages(Guid itemId)
    {
        var login = userContext.Login;
        var imagesPath = Path.Combine(WebRootPath.Path, "images", login, itemId.ToString());

        if (!Directory.Exists(imagesPath)) return [];

        var imageFileNames = Directory.GetFiles(imagesPath)
            .Select(Path.GetFileName)
            .ToList();
        
        return imageFileNames;
    }
}
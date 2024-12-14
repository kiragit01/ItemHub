using ItemHub.Interfaces;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class UserApiService(
    IUserRepository userRepository,
    IItemRepository itemRepository,
    ICacheRepository cacheRepository)
    : IUserApiService
{
    public async Task<int> GetFavoritedItemsCountAsync()
    {
        var user = await userRepository.GetUserAsync();
        return user?.FavoritedItemsId.Count ?? 0;
    }

    public async Task<List<Guid>> GetFavoritedItemsAsync()
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return [];
        var validItems = new List<Guid>();
        var itemsToRemove = new List<Guid>();
        foreach (var id in user.FavoritedItemsId)
        {
            var itemExists = await itemRepository.ItemExistsAsync(id);
            if (itemExists) validItems.Add(id);
            else itemsToRemove.Add(id);
        }

        await cacheRepository.RemoveAsync(user.Login);
        if (itemsToRemove.Count <= 0) return validItems;
        foreach (var id in itemsToRemove)
            user.FavoritedItemsId.Remove(id);
        await userRepository.UpdateUserAsync(user);
        return validItems;
    }

    public async Task<Result<bool>> ToggleFavoriteItemAsync(Guid itemId)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null)
            return Result<bool>.Fail("Пользователь не найден.");

        bool isFavorited;

        if (user.FavoritedItemsId.Contains(itemId))
        {
            user.FavoritedItemsId.Remove(itemId);
            isFavorited = false;
        }
        else
        {
            user.FavoritedItemsId.Add(itemId);
            isFavorited = true;
        }

        await userRepository.UpdateUserAsync(user);
        return Result<bool>.Ok(isFavorited);
    }
}
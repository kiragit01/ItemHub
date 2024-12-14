using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IUserApiService
{
    Task<int> GetFavoritedItemsCountAsync();
    Task<List<Guid>> GetFavoritedItemsAsync();
    Task<Result<bool>> ToggleFavoriteItemAsync(Guid itemId);
}
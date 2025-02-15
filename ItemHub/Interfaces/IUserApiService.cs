using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IUserApiService
{
    Task<Result<bool>> UpdateFavoritesItemAsync(List<Guid> itemsId);
}
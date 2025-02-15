using ItemHub.Interfaces;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class UserApiService(IUserRepository userRepository)
    : IUserApiService
{
    public async Task<Result<bool>> UpdateFavoritesItemAsync(List<Guid> itemsId)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null)
            return Result<bool>.Fail("Пользователь не найден.");

        if (user.FavoritedItemsId == itemsId) return Result<bool>.Ok(true);
        user.FavoritedItemsId = itemsId.Distinct().ToList();
        await userRepository.UpdateUserAsync(user);
        return Result<bool>.Ok(true);
    }
}
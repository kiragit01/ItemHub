using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Utilities;
namespace ItemHub.Services;

public class UserAccountService(
    IUserRepository userRepository,
    IItemRepository itemRepository,
    ICacheRepository cacheRepository,
    IUserContext userContext, 
    IMyCookieManager cookieManager)
    : IUserAccountService
{
    public async Task<User?> GetUser(string? login = null) =>
           await cacheRepository.GetAsync<User>(login ?? userContext.Login)
        ?? await userRepository.GetUserAsync(login);
    
    public async Task<string?> EditAccount(EditAccountModel model)
    {
        var check = await userRepository.CheckUserAsync(model.Email, model.Login);
        var error = check switch
        {
            ResponseMessage.ErrorEmail when model.Email != userContext.Email => "Этот Email занят.",
            ResponseMessage.ErrorLogin when model.Login != userContext.Login => "Этот логин занят",
            _ => null
        };

        var user = await userRepository.GetUserAsync();
        if (user == null) return "Пользователь не найден";
        if (user.Login != model.Login)
        {
            await itemRepository.RenameItemsUserAsync(user.CustomItems, model.Login);
            var itemCacheKeys = user.CustomItems.Select(item => $"Item_{item.Id}").ToList();
            foreach (var key in itemCacheKeys)
            {
                await cacheRepository.RemoveAsync(key);
            }
        }
        await cacheRepository.RemoveAsync("index");
        await cacheRepository.RemoveAsync(user.Login);
        var avatar = model.Avatar == null 
            ? user.Avatar 
            : await UploadFiles.UploadAvatarAsync(model.Avatar, model.Login);
        
        user.UpdateDataUser(
            model.Name, 
            model.Login, 
            model.Email, 
            avatar, 
            model.Description, 
            model.Phone);

        await userRepository.UpdateUserAsync(user);
        await cookieManager.AuthenticationAsync(user);
        return error;
    }
    
    public async Task<string?> UpdatePassword(UpdatePasswordModel model)
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return "Пользователь не найден";
        var hashedPassword = HashedPassword.Hashed(model.OldPassword, user.Salt);
        if (hashedPassword != user.HashedPassword) 
            return "Старый пароль неверный.";
        user.Salt = HashedPassword.GeneratedSalt;
        user.HashedPassword = HashedPassword.Hashed(model.NewPassword, user.Salt);
        await userRepository.UpdateUserAsync(user);
        return null;  
    }
    
    public async Task<ResponseMessage> DeleteAccount()
    {
        var user = await userRepository.GetUserAsync();
        if (user == null) return ResponseMessage.Error;
        await itemRepository.RemoveItemsUserAsync(user.CustomItems);
        await userRepository.DeleteUserAsync(user);
        await cacheRepository.RemoveAsync(user.Login);
        await cookieManager.SignOutAsync();
        return ResponseMessage.Ok;
    }
}
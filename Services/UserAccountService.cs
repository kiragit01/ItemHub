using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;
namespace ItemHub.Services;

public class UserAccountService(
    IUserRepository userRepository, 
    IItemRepository itemRepository, 
    IUserContext userContext, 
    IMyCookieManager cookieManager)
    : IUserAccountService
{
    public async Task<User?> GetUser() => await userRepository.GetUserAsync();
    
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

        foreach (var item in user.CustomItems)
        {
            var itemDb = await itemRepository.GetItemAsync(item.Id);
            if (itemDb == null) continue;
            itemDb.Creator = model.Login;
            await itemRepository.UpdateItemAsync(itemDb);
        }

        var avatar = model.Avatar == null 
            ? user.Avatar 
            : await UploadFiles.UploadAvatar(model.Avatar, model.Login);
        user.UpdateDataUser(model.Name, 
            model.Login, model.Email, avatar, 
            model.Description, model.Phone);

        await userRepository.UpdateUserAsync(user);
        await cookieManager.Authentication(user);
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
        foreach (var item in user.CustomItems)
        {
            var itemDb = await itemRepository.GetItemAsync(item.Id);
            if (itemDb != null) await itemRepository.RemoveItemAsync(itemDb);
        }
        await userRepository.DeleteUserAsync(user);
        await cookieManager.SignOutAsync();
        return ResponseMessage.Ok;
    }
}
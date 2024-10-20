using ItemHub.Models.Auth;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;
namespace ItemHub.Services;

public static class UserManagementService
{
    public static async Task<string?> EditAccount(string login, string email, EditAccountModel model, 
        IUserDb dbU, IItemDb dbI, string webRootPath, HttpContext httpContext)
    {
        var check = await dbU.CheckUser(model.Email, model.Login);
        var error = check switch
        {
            ResponseMessage.ErrorEmail when model.Email != email => "Этот Email занят.",
            ResponseMessage.ErrorLogin when model.Login != login => "Этот логин занят",
            _ => null
        };

        var user = await dbU.GetUser(login);
        if (user == null) return "Пользователь не найден";

        foreach (var item in user.CustomItems)
        {
            var itemDb = await dbI.GetItem(item.Id);
            if (itemDb == null) continue;
            itemDb.Creator = model.Login;
            await dbI.UpdateItem(itemDb);
        }

        var avatar = model.Avatar == null 
            ? user.Avatar 
            : await UploadFiles.UploadAvatar(model.Avatar, model.Login, webRootPath);
        user.UpdateDataUser(model.Name, 
            model.Login, model.Email, avatar, 
            model.Description, model.Phone);

        await dbU.UpdateUser(user);
        await CookieManager.Authentication(user, httpContext);
        return error;
    }
    
    public static async Task<string?> UpdatePassword(string login, UpdatePasswordModel model, IUserDb dbU)
    {
        var user = await dbU.GetUser(login);
        if (user == null) return "Пользователь не найден";
        var hashedPassword = HashedPassword.Hashed(model.OldPassword, user.Salt);
        if (hashedPassword != user.HashedPassword) 
            return "Старый пароль неверный.";
        user.Salt = HashedPassword.GeneratedSalt;
        user.HashedPassword = HashedPassword.Hashed(model.NewPassword, user.Salt);
        
        await dbU.UpdateUser(user);
        return null;  
    }
}
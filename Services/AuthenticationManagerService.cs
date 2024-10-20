using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ItemHub.Services;

public static class AuthenticationManagerService
{
    public static async Task<ResponseMessage> Login(LoginModel model, IUserDb dbU, HttpContext httpContext)
    {
        var user = await dbU.LogIn(model.Login, model.Password);
        if (user == null) return ResponseMessage.Error;
        await CookieManager.Authentication(user, httpContext);
        return ResponseMessage.Ok;
    }
    
    public static async Task<string?> Register(RegisterModel model, IUserDb dbU, string webRootPath, HttpContext httpContext)
    {
        var debug = await dbU.CheckUser(model.Email, model.Login);
        var error = debug switch
        {
            ResponseMessage.ErrorEmail => "Этот Email занят.",
            ResponseMessage.ErrorLogin => "Этот логин занят.",
            _ => null
        };
        var avatar = await UploadFiles.UploadAvatar(model.Avatar, model.Login, webRootPath);
        var salt = HashedPassword.GeneratedSalt;
        var hashedPassword = HashedPassword.Hashed(model.Password, salt);
        var user = new User(model.Name, model.Login, model.Email, hashedPassword, salt, avatar);

        if (model.Seller) user.AddRoles([UserRoles.SELLER]);
        else user.AddRoles([UserRoles.CUSTOMER]);

        await dbU.AddUser(user);
        await CookieManager.Authentication(user, httpContext);
        return error;
    }
    
    public static async Task<ResponseMessage> DeleteAccount(string login,IUserDb dbU, IItemDb dbI, HttpContext httpContext)
    {
        var user = await dbU.GetUser(login);
        if (user == null) return ResponseMessage.Error;
        foreach (var item in user.CustomItems)
        {
            var itemDb = await dbI.GetItem(item.Id);
            if (itemDb != null) await dbI.RemoveItem(itemDb);
        }
        await dbU.DeleteUser(user);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return ResponseMessage.Ok;
    }
}
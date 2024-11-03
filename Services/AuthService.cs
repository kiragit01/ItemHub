using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class AuthService(IUserRepository userRepository, IMyCookieManager cookieManager) : IAuthService
{
    public async Task<ResponseMessage> Login(LoginModel model)
    {
        var user = await userRepository.LogInAsync(model.Login, model.Password);
        if (user == null) return ResponseMessage.Error;
        await cookieManager.Authentication(user);
        return ResponseMessage.Ok;
    }
    
    public async Task<string?> Register(RegisterModel model)
    {
        var debug = await userRepository.CheckUserAsync(model.Email, model.Login);
        var error = debug switch
        {
            ResponseMessage.ErrorEmail => "Этот Email занят.",
            ResponseMessage.ErrorLogin => "Этот логин занят.",
            _ => null
        };
        var avatar = await UploadFiles.UploadAvatar(model.Avatar, model.Login);
        var salt = HashedPassword.GeneratedSalt;
        var hashedPassword = HashedPassword.Hashed(model.Password, salt);
        var user = new User(model.Name, model.Login, model.Email, hashedPassword, salt, avatar);

        if (model.Seller) user.AddRoles([UserRoles.SELLER]);
        else user.AddRoles([UserRoles.CUSTOMER]);

        await userRepository.AddUserAsync(user);
        await cookieManager.Authentication(user);
        return error;
    }
    
    public async Task Logout() => await cookieManager.SignOutAsync();
}
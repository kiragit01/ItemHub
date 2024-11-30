using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Utilities;

namespace ItemHub.Services;

public class AuthService(
    IUserRepository userRepository,
    ICacheRepository cacheRepository,
    IUserContext userContext,
    IMyCookieManager cookieManager) 
    : IAuthService
{
    public async Task<ResponseMessage> Login(LoginModel model)
    {
        var user = await userRepository.LogInAsync(model.Login, model.Password);
        if (user == null) return ResponseMessage.Error;
        await cacheRepository.SetAsync(user.Login, user, Constants.UserCacheSlidingExpiration);
        await cookieManager.AuthenticationAsync(user);
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
        if (error != null) return error;
        var avatar = UploadFiles.UploadAvatarAsync(model.Avatar, model.Login);
        var salt = HashedPassword.GeneratedSalt;
        var hashedPassword = HashedPassword.Hashed(model.Password, salt);
        var user = new User(model.Name, model.Login, model.Email, hashedPassword, salt, avatar.Result);

        if (model.Seller) user.AddRoles([UserRoles.SELLER]);
        else user.AddRoles([UserRoles.CUSTOMER]);

        await userRepository.AddUserAsync(user);
        await cacheRepository.SetAsync(user.Login, user, Constants.UserCacheSlidingExpiration);
        await cookieManager.AuthenticationAsync(user);
        return null;
    }
    
    public async Task Logout()
    {
        await cacheRepository.RemoveAsync(userContext.Login);
        await cookieManager.SignOutAsync();
    }
}
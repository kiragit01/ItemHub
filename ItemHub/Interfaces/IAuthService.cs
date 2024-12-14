using ItemHub.Models.Auth;
using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IAuthService
{
    public Task<ResponseMessage> Login(LoginModel model);
    public Task<string?> Register(RegisterModel model);
    public Task Logout();
}
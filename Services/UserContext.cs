using ItemHub.Interfaces;
using System.Security.Claims;
namespace ItemHub.Services;

public class UserContext : IUserContext
{
    public string Login { get; }
    public string Email { get; }
    public string Name { get; }

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        Login = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        Email = user?.FindFirst(ClaimTypes.Email)?.Value!;
        Name = user?.FindFirst(ClaimTypes.Name)?.Value!;
    }
}
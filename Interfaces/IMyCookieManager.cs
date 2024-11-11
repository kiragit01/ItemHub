using ItemHub.Models.User;

namespace ItemHub.Interfaces;

public interface IMyCookieManager
{
    public Task AuthenticationAsync(User user);
    public Task SignOutAsync();
}
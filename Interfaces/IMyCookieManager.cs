using ItemHub.Models.User;

namespace ItemHub.Interfaces;

public interface IMyCookieManager
{
    public Task Authentication(User user);
    public Task SignOutAsync();
}
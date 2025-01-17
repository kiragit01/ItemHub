using ItemHub.Models.User;

namespace ItemHub.Interfaces;

public interface IMyCookieManager
{
    public Task AuthenticationAsync(User user);
    public Task SignOutAsync();
    public string GetOrCreateUniqueViewId();
    public DateTime? GetLastViewedTimeFromCookie(Guid itemId);
    public void UpdateViewTimeInCookie(Guid itemId);
}
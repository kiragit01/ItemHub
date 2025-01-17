using System.Diagnostics;
using System.Security.Claims;
using ItemHub.Models.User;
using ItemHub.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

namespace ItemHub.Services;

public class CookieManager(IHttpContextAccessor httpContextAccessor) : IMyCookieManager
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;
    public async Task AuthenticationAsync(User user)
    {
        // создаем claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String),
            new Claim(ClaimTypes.NameIdentifier, user.Login, ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        // создаем объект ClaimsIdentity
        var id = new ClaimsIdentity(claims, "ApplicationCookie", 
            ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        // установка аутентификационных куки
        await _httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }

    public async Task SignOutAsync() 
        => await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    public string GetOrCreateUniqueViewId()
    {
        if (_httpContext.User.Identity != null && _httpContext.User.Identity.IsAuthenticated)
        {
            return _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "authenticated_user";
        }
        if (_httpContext.Request.Cookies.ContainsKey("UniqueViewId"))
        {
            return _httpContext.Request.Cookies["UniqueViewId"]!;
        }
        var uniqueId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };
        _httpContext.Response.Cookies.Append("UniqueViewId", uniqueId, cookieOptions);
        return uniqueId;
    }
    
    public DateTime? GetLastViewedTimeFromCookie(Guid itemId)
    {
        var cookieName = "ViewedItems";
        if (!_httpContext.Request.Cookies.ContainsKey(cookieName)) return null;
        var protectedData = _httpContext.Request.Cookies[cookieName];
        try
        {
            var viewedItems = JsonConvert.DeserializeObject<Dictionary<Guid, DateTime>>(protectedData!);
            if (viewedItems != null && viewedItems.ContainsKey(itemId))
            {
                return viewedItems[itemId];
            }
        }
        catch (JsonException)
        {
            Debug.Fail($"Unable to parse viewed item: {protectedData}. Ошибки десериализации");
        }
        return null;
    }
    
    public void UpdateViewTimeInCookie(Guid itemId)
    {
        var cookieName = "ViewedItems";
        Dictionary<Guid, DateTime> viewedItems;

        if (_httpContext.Request.Cookies.ContainsKey(cookieName))
        {
            var protectedData = _httpContext.Request.Cookies[cookieName];
            try
            {
                viewedItems = JsonConvert.DeserializeObject<Dictionary<Guid, DateTime>>(protectedData!)
                              ?? new Dictionary<Guid, DateTime>();
            }
            catch (JsonException)
            {
                viewedItems = new Dictionary<Guid, DateTime>();
            }
        }
        else
        {
            viewedItems = new Dictionary<Guid, DateTime>();
        }

        // Обновляем или добавляем время просмотра
        viewedItems[itemId] = DateTime.UtcNow;

        // Ограничиваем количество записей до 100
        if (viewedItems.Count > 100)
        {
            viewedItems = viewedItems
                .OrderBy(kvp => kvp.Value)
                .TakeLast(100)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        var serializedData = JsonConvert.SerializeObject(viewedItems);

        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };
        _httpContext.Response.Cookies.Append(cookieName, serializedData, cookieOptions);
    }
}
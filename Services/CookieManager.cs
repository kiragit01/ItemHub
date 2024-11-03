using System.Security.Claims;
using ItemHub.Models.User;
using ItemHub.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ItemHub.Services;

public class CookieManager(IHttpContextAccessor httpContextAccessor) : IMyCookieManager
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;
    public async Task Authentication(User user)
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
}
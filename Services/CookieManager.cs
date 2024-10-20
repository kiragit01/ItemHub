using System.Security.Claims;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ItemHub.Services;

public class CookieManager
{
    public static async Task Authentication(User user, HttpContext httpContext)
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
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
}
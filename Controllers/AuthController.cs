using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace ItemHub.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    [HttpGet("register")]
    public IActionResult Register() => View();

    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpPost("register")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        var error = await authService.Register(model);
        return error != null 
            ? ModelError(error) 
            : RedirectToAction("Index", "Home");
    }

    [HttpPost("login")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        return await authService.Login(model) == ResponseMessage.Error 
            ? ModelError("Неверный логин и(или) пароль.") 
            : RedirectToAction("Index", "Home");
    }
    
    
    public async Task<IActionResult> Logout()
    {
        await authService.Logout();
        return RedirectToAction("Login", "Auth");
    }
    
    private IActionResult ModelError(string error = "Данные заполнены неверно.")
    {
        ModelState.AddModelError("", error);
        return View();
    }
}
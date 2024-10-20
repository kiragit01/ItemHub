using ItemHub.Models.Auth;
using ItemHub.Repository.Interfaces;
using ItemHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ItemHub.Controllers;

public class AuthController(IUserDb dbU, IWebHostEnvironment webHostEnvironment) : Controller
{
    private readonly string _webRootPath = webHostEnvironment.WebRootPath;
        
    [HttpGet] [Route("register")]
    public IActionResult Register() => View();

    [HttpGet] [Route("login")]
    public IActionResult Login() => View();

    [HttpPost] [Route("register")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        var error = await AuthenticationManagerService.Register(model, dbU, _webRootPath, HttpContext);
        return error != null 
            ? ModelError(error) 
            : RedirectToAction("Index", "Home");
    }

    [HttpPost] [Route("login")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        return await AuthenticationManagerService.Login(model, dbU, HttpContext) == ResponseMessage.Error 
            ? ModelError("Неверный логин и(или) пароль.") 
            : RedirectToAction("Index", "Home");
    }
    
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }
    
    private IActionResult ModelError(string error = "Данные заполнены неверно.")
    {
        ModelState.AddModelError("", error);
        return View();
    }
}
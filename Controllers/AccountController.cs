using Microsoft.AspNetCore.Mvc;
using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers;

public class AccountController(IUserAccountService userAccountService) : Controller
{
    [HttpGet] 
    [Route("profile/{login?}")]
    public async Task<IActionResult> Account(string? login)
    {
        var user = await userAccountService.GetUser(login);
        return user == null 
            ? RedirectToAction("Index", "Home") //BadRequest("Такого пользователя не существует")
            : View(user);
    }
        
    [HttpGet] [Authorize]
    [Route("account/edit")] 
    public async Task<IActionResult> EditAccount()
    {
        var user = await userAccountService.GetUser();
        if (user == null) return RedirectToAction("Login", "Auth");
        var editUser = new EditAccountModel(user.Name, user.Login, user.Email, user.Description, user.Phone);
        return View(editUser);
    }

    [HttpPost] [Route("account/edit")] 
    [ValidateAntiForgeryToken] [Authorize]
    public async Task<IActionResult> EditAccount(EditAccountModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        var error = await userAccountService.EditAccount(model);
        return error != null 
            ? ModelError(error) 
            : RedirectToAction("Index", "Home");
    }
        
    [HttpPost] [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        return
            await userAccountService.DeleteAccount() == ResponseMessage.Error
                ? BadRequest("Войдите в аккаунт для того чтобы его удалить.")
                : RedirectToAction("Login", "Auth");
    }

    [HttpPost] [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
    {
        if (!ModelState.IsValid) return ModelError();
        var error = await userAccountService.UpdatePassword(model);
        return error != null 
            ? ModelError(error) 
            : RedirectToAction("Account", "Account");
    }
        
    private IActionResult ModelError(string error = "Данные заполнены неверно.")
    {
        ModelState.AddModelError("", error);
        return View();
    }
}
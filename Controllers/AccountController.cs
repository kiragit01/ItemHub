using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ItemHub.Interfaces;
using ItemHub.Models.Auth;
using ItemHub.Services;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{
    [Authorize] 
    public class AccountController(IUserAccountService userAccountService) : Controller
    {
        
        [HttpGet] 
        [Route("account")] 
        public async Task<IActionResult> Account()
        {
            var user = await userAccountService.GetUser();
            return View(user);
        }
        
        [HttpGet]
        [Route("account/edit")] 
        public async Task<IActionResult> EditAccount()
        {
            var user = await userAccountService.GetUser();
            if (user == null) return RedirectToAction("Login", "Auth");
            var editUser = new EditAccountModel(user.Name, user.Login, user.Email, user.Description, user.Phone);
            return View(editUser);
        }

        [HttpPost] [Route("account/edit")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccountModel model)
        {
            if (!ModelState.IsValid) return ModelError();
            var error = await userAccountService.EditAccount(model);
            return error != null 
                ? ModelError(error) 
                : RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            return
                await userAccountService.DeleteAccount() == ResponseMessage.Error
                ? BadRequest("Войдите в аккаунт для того чтобы его удалить.")
                : RedirectToAction("Login", "Auth");
        }

        [HttpPost]
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
}

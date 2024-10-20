using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ItemHub.Models.Auth;
using ItemHub.Repository.Interfaces;
using ItemHub.Services;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{
    [Authorize] 
    public class AccountController(IUserDb dbU, IItemDb dbI, IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly string _webRootPath = webHostEnvironment.WebRootPath;
        
        [HttpGet] 
        [Route("account")] 
        public async Task<IActionResult> Account()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return View(user);
        }
        
        [HttpGet]
        [Route("account/edit")] 
        public async Task<IActionResult> EditAccount()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return RedirectToAction("Login", "Auth");
            var editUser = new EditAccountModel(user.Name, user.Login, user.Email, user.Description, user.Phone);
            return View(editUser);
        }

        [HttpPost] [Route("account/edit")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccountModel model)
        {
            if (!ModelState.IsValid) return ModelError();
            var error = await UserManagementService.EditAccount(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                User.FindFirst(ClaimTypes.Email)!.Value,
                model, dbU, dbI, _webRootPath, HttpContext);
            return error != null 
                ? ModelError(error) 
                : RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var login = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return
                await AuthenticationManagerService.DeleteAccount(
                    login, dbU, dbI, HttpContext) == ResponseMessage.Error
                ? BadRequest("Войдите в аккаунт для того чтобы его удалить.")
                : RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (!ModelState.IsValid) return ModelError();
            var login = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var error = await UserManagementService.UpdatePassword(login, model, dbU);
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

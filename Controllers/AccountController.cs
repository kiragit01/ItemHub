using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Repository;
using ItemHub.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ItemHub.Utilities;

namespace ItemHub.Controllers
{
    public class AccountController(IUserDb dbU, IItemDb dbI, IWebHostEnvironment webHostEnvironment) : Controller
    {
        [HttpGet]
        [Route("register")]
        public IActionResult Register() => View();

        [HttpGet]
        [Route("login")]
        public IActionResult Login() => View();

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, IFormFileCollection test)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Что-то пошло не так");
                return View();
            }

            var debug = await dbU.CheckUser(model.Email, model.Login);
            switch (debug)
            {
                case DebugMessage.ErrorEmail:
                    ModelState.AddModelError("", "Этот Email уже зарегистрирован");
                    return View();
                case DebugMessage.ErrorLogin:
                    ModelState.AddModelError("", "Этот логин занят");
                    return View();
            }

            var avatar = await UploadAvatar(model.Avatar, model.Login);
            var salt = HashedPassword.GeneratedSalt;
            var hashedPassword = HashedPassword.Hashed(model.Password, salt);
            var user = new User(model.Name, model.Login, model.Email, hashedPassword, salt, avatar);

            if (model.Seller) user.AddRoles([UserRoles.SELLER]);
            else user.AddRoles([UserRoles.CUSTOMER]);

            await dbU.AddUser(user);
            await Authenticate(user); // аутентификация

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View();
            User? user = await dbU.SingIn(model.Login, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин и(или) пароль.");
                return View();
            }

            await Authenticate(user); // аутентификация
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [Route("account")]
        [Authorize]
        public async Task<IActionResult> Account()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return View(user);
        }


        [HttpGet]
        [Route("account/edit")]
        [Authorize]
        public async Task<IActionResult> EditAccount()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return RedirectToAction("Login");
            ViewBag.editUser = new EditAccountModel
            {
                Name = user.Name,
                Login = user.Login,
                Email = user.Email,
                Description = user.Description,
                Phone = user.Phone
            };
            return View();
        }

        [HttpPost]
        [Route("account/edit")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccountModel model)
        {
            if (!ModelState.IsValid) return View();
            var check = await dbU.CheckUser(model.Email, model.Login);
            if (check == DebugMessage.ErrorEmail && model.Email != User.FindFirst(ClaimTypes.Email)!.Value)
            {
                ModelState.AddModelError("", "Этот Email уже зарегистрирован");
                return View();
            }

            if (check == DebugMessage.ErrorLogin && model.Login != User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
            {
                ModelState.AddModelError("", "Этот логин занят");
                return View();
            }

            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return RedirectToAction("Index", "Home");

            foreach (var item in user.CustomItems)
            {
                var itemDb = await dbI.GetItem(item.Id);
                if (itemDb == null) continue;
                itemDb.Creator = model.Login;
                await dbI.UpdateItem(itemDb);
            }

            var avatar = model.Avatar == null ? user.Avatar : await UploadAvatar(model.Avatar, model.Login);
            user.Name = model.Name;
            user.Login = model.Login;
            user.Email = model.Email;
            user.Avatar = avatar;
            user.Description = model.Description;
            user.Phone = model.Phone;

            await dbU.UpdateUser(user);
            await Authenticate(user); // аутентификация

            return RedirectToAction("Index", "Home");
        }

        private async Task Authenticate(User user)
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
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (user == null) return BadRequest("Войдите в аккаунт для того чтобы его удалить.");
            
            foreach (var item in user.CustomItems)
            {
                var itemDb = await dbI.GetItem(item.Id);
                if (itemDb != null) await dbI.RemoveItem(itemDb);
            }

            await dbU.DeleteUser(user);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return RedirectToAction("Login");
            var hashedPassword = HashedPassword.Hashed(model.OldPassword, user.Salt);
            if (hashedPassword != user.HashedPassword)
            {
                ModelState.AddModelError("", "Старый пароль неверный.");
                return BadRequest();
            }

            user.Salt = HashedPassword.GeneratedSalt;
            user.HashedPassword = HashedPassword.Hashed(model.NewPassword, user.Salt);
            
            await dbU.UpdateUser(user);
            return RedirectToAction("Account", "Account");  
        }
        
        //Создание пути к Аватарке
        private async Task<string> UploadAvatar(IFormFile? file, string userLogin)
        {
            if (file == null) return "images/NoAvatar.png";
            // создаем папку для хранения файлов
            Directory.CreateDirectory( Path.Combine(webHostEnvironment.WebRootPath, "images", userLogin));
            var type = "." + file.ContentType.Split("/")[1];
            // путь к файлу
            var pathImages = Path.Combine("images", userLogin, "Avatar" + type);
            var fullPath = Path.Combine(webHostEnvironment.WebRootPath, pathImages);
            // сохраняем файл
            var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return pathImages;
        }
        


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        
        public IActionResult AccessDenied() => RedirectToAction("Index", "Home");
    }
}

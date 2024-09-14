using ItemHub.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ItemHub.Models.Auth;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{
    public class AccountController(UserContext db) : Controller
    {
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View();
            User? check = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (check != null)
            {
                ModelState.AddModelError("", "Этот Email уже зарегистрирован");
                return View();
            }
            check = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login);
            if (check != null)
            {
                ModelState.AddModelError("", "Этот логин занят");
                return View();
            }

            var user = new User(model.Login, model.Password, model.Name, model.Email, model.Age.Value);

            if (model.Seller) user.AddRoles([UserRoles.SELLER]);
            else user.AddRoles([UserRoles.CUSTOMER]);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            await Authenticate(user); // аутентификация

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User? user = await db.Users
                    .FirstOrDefaultAsync(u => (u.Login == model.Login || u.Email == model.Login) && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(user); // аутентификация
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неверный логин и(или) пароль.");
            }
            return View();
        }

        
        [HttpGet]
        [Route("account")]
        [Authorize]
        public async Task<IActionResult> Account()
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Login == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return View(user);
        }
        
        
        [HttpGet]
        [Route("account/edit")]
        [Authorize]
        public async Task<IActionResult> EditAccount()
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Login == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return RedirectToAction("Login");
            var editUser = new EditAccModel
            {
                Login = user.Login,
                Password = user.Password,
                Name = user.Name,
                Email = user.Email
            };
            ViewBag.editUser = editUser;
            return View();
        }

        [HttpPost]
        [Route("account/edit")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccModel model)
        {
            if (!ModelState.IsValid) return View();
            User? check = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (check != null && check?.Email != User.FindFirst(ClaimTypes.Email)?.Value)
            {
                ModelState.AddModelError("", "Этот Email уже зарегистрирован");
                return View();
            }
            check = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login);
            if (check != null && check?.Login != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                ModelState.AddModelError("", "Этот логин занят");
                return View();
            }
            
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Login == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (user == null) return RedirectToAction("Index", "Home");
            
            user.Login = model.Login;
            user.Email = model.Email;
            user.Name = model.Name;
            user.Password = model.Password;

            db.Users.Update(user);
            await db.SaveChangesAsync();
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
                new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email),
                new Claim("Age", user.Age.ToString(), ClaimValueTypes.Integer)
            };
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // создаем объект ClaimsIdentity
            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        
        
        
        

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


        public IActionResult AccessDenied() => RedirectToAction("Index", "Home");
    }
}

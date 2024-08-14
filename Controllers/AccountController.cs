using ItemHub.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using ItemHub.Models.Auth;
using ItemHub.Models.User;

namespace ItemHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserContext db;
        public AccountController(UserContext context)
        {
            db = context;
        }

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
            if (ModelState.IsValid) 
            {
                User? Check;
                Check = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (Check != null)
                {
                    ModelState.AddModelError("", "Этот Email уже зарегистрирован");
                    return View();
                }
                Check = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login);
                if (Check != null)
                {
                    ModelState.AddModelError("", "Этот логин занят");
                    return View();
                }

                User user = new User(login: model.Login, password: model.Password, name: model.Name, email: model.Email, age: model.Age.Value);

                if (model.Seller) user.AddRoles([UserRoles.SELLER]);
                else user.AddRoles([UserRoles.CUSTOMER]);

                db.Users.Add(user);
                await db.SaveChangesAsync();

                await Authenticate(user); // аутентификация

                return RedirectToAction("Index", "Home");
            }
            return View();
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
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

    }
}

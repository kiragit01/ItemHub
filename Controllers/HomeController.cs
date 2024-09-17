using ItemHub.Data;
using ItemHub.Models;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserContext db;

        public HomeController(ILogger<HomeController> logger, UserContext context)
        {
            _logger = logger;
            db = context;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.Items = await ListItems();
            return View();
        }
        
        [Route("my")]
        [Authorize(Roles = UserRoles.SELLER)]
        public async Task<IActionResult> MyItems()
        {
            var user = await db.Users
                .Include(user => user.Items)
                .FirstOrDefaultAsync(u => u.Login == User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return BadRequest("Войдите в аккаунт.");
            ViewBag.Items = user.Items;
            return View();
        }

        private async Task<List<Item>> ListItems() => await db.Items.ToListAsync();

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

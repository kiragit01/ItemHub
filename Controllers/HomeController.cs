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
            // foreach (var t in ViewBag.Items)
            // {
            //     Console.WriteLine($"{t.Title} - {t.Description} - {t.Price}");
            // }
            return View();
        }

        public async Task<List<Item>> ListItems() => await db.Items.ToListAsync();

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

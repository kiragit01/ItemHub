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
using ItemHub.DbConnection.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{

    public class HomeController(ILogger<HomeController> logger, IUserDb dbU, IItemDb dbI) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        private readonly int _pageSize = 2; // количество элементов на странице
        public async Task<IActionResult> Index(int page = 1)
        {
            var source = dbI.AllItems();
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * _pageSize).Take(_pageSize).ToListAsync();
 
            var pageViewModel = new PageViewModel(count, page, _pageSize);
            var viewModel = new IndexViewModel(items, pageViewModel);
            ViewBag.ViewPageItems = viewModel;
            return View();
        }
        
        [Route("my")]
        [Authorize(Roles = UserRoles.SELLER)]
        public async Task<IActionResult> MyItems(int page = 1)
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return BadRequest("Войдите в аккаунт.");

            var userItems = user.Items;
            var items = userItems.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();
            
            var pageViewModel = new PageViewModel(userItems.Count, page, _pageSize);
            var viewModel = new IndexViewModel(items, pageViewModel);
            ViewBag.ViewPageItems = viewModel;
            return View();
        }

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

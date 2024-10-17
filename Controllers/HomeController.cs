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
using ItemHub.Models.Pages;
using ItemHub.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers
{

    public class HomeController(IUserDb dbU, IItemDb dbI) : Controller
    {
        private const int PageSize = 2; // количество элементов на странице

        public async Task<IActionResult> Index(int page = 1)
        {
            var source = dbI.AllItems();
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
 
            var pageViewModel = new PageViewModel(count, page, PageSize);
            var viewModel = new IndexViewModel(items, pageViewModel);
            return View(viewModel);
        }
        
        [Route("my")]
        [Authorize(Roles = UserRoles.SELLER)]
        public async Task<IActionResult> MyItems(int page = 1)
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return BadRequest("Войдите в аккаунт.");

            var userItems = user.CustomItems;
            var items = userItems.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            
            var pageViewModel = new PageViewModel(userItems.Count, page, PageSize);
            var viewModel = new IndexViewModel(items, pageViewModel);
            return View(viewModel);
        }
        
        [Route("favorite")]
        [Authorize(Roles = UserRoles.CUSTOMER)]
        public async Task<IActionResult> FavoritedItems(int page = 1)
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return BadRequest("Войдите в аккаунт.");

            var favoritedItems = new List<Item>();
            var validItemIds = new List<Guid>();

            foreach (var id in user.FavoritedItemsId)
            {
                var item = await dbI.GetItemNoTracking(id);
                if (item == null) continue;
                favoritedItems.Add(item);
                validItemIds.Add(id);
            }

            user.FavoritedItemsId = validItemIds;
            await dbU.UpdateUser(user);

            var items = favoritedItems.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            
            var pageViewModel = new PageViewModel(favoritedItems.Count, page, PageSize);
            var viewModel = new IndexViewModel(items!, pageViewModel);
            return View(viewModel);
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

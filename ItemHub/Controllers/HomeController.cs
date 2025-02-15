using ItemHub.Models;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ItemHub.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers;

public class HomeController(IPageManagerService pageManagerService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetMaxPrice()
    {
        var maxPrice = await pageManagerService.MaxPrice();
        return Json(new { maxPrice });
    }

    [HttpGet]
    public async Task<IActionResult> SearchItemsAjax(
        string query, int? minPrice, int? maxPrice, int? page, bool onlyMine = false, bool favorite = false)
    {
        var indexViewModel = await pageManagerService.SearchItem(query, minPrice, maxPrice, page, onlyMine, favorite);
        return PartialView("_SearchResultsPartial", indexViewModel);
    }

    [Route("my")]
    [Authorize(Roles = UserRoles.SELLER)]
    public async Task<IActionResult> MyItems()
    {
        return View();
    }
    
    [Route("favorite")]
    [Authorize(Roles = UserRoles.CUSTOMER)]
    public async Task<IActionResult> FavoritedItems()
    {
        return View();
    }
    
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
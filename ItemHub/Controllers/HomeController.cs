using ItemHub.Models;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ItemHub.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ItemHub.Controllers;

public class HomeController(IPageManagerService pageManagerService) : Controller
{
    public async Task<IActionResult> Index(int? page)
    {
        var viewModel = await pageManagerService.Index(page);
        return View(viewModel);
    }
        
    [Route("my")]
    [Authorize(Roles = UserRoles.SELLER)]
    public async Task<IActionResult> MyItems(int? page)
    {
        var viewModel = await pageManagerService.MyItems(page);
        return View(viewModel);
    }
    
    [Route("favorite")]
    [Authorize(Roles = UserRoles.CUSTOMER)]
    public async Task<IActionResult> FavoritedItems(int? page)
    {
        var viewModel = await pageManagerService.FavoritedItems(page);
        return View(viewModel);
    }
    
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
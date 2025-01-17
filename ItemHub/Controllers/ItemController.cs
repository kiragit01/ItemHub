using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ItemHub.Interfaces;

namespace ItemHub.Controllers;

public class ItemController(IItemService itemService) : Controller
{
    [HttpGet("/item")]
    public async Task<ActionResult> ViewItem(Guid id)
    {
        var item = await itemService.GetItemNoTracking(id);
        if (item == null) BadResult();
        await itemService.RegisterViewAsync(id);
        return View(item);
    }

    [HttpGet("/create")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public ActionResult Create() => View();

    [HttpPost("/create")] [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public async Task<ActionResult> Create(ItemModel model)
    {
        if(!ModelState.IsValid) return View();
        var error = await itemService.CreateAsync(model);
        return error != null
            ? BadResult(error)
            : RedirectToAction("Index", "Home");
    }
        
    [HttpGet("/edit")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public async Task<ActionResult> Edit(Guid id)
    {
        var item = await itemService.GetItemNoTracking(id);
        if (item == null) return BadRequest("Такого товара не существует :(");
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator 
            ? RedirectToAction("ViewItem", "Item", new { id })
            : View(item);
    }

    [HttpPut("/update")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Update(Guid id, ItemModel model, List<string> savedImages)
    {
        if (!ModelState.IsValid) return RedirectToAction("Edit");
        var error = await itemService.UpdateAsync(id, model, savedImages);
        return error == null 
            ? RedirectToAction("ViewItem", "Item", new { id })
            : BadRequest(error);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(Guid id)
    {
        var error = await itemService.DeleteAsync(id);
        return error == null 
            ? RedirectToAction("Index","Home")
            : BadResult(error);
    }

    private ActionResult BadResult(string error = "Что-то пошло не так :(") => BadRequest(error);
}
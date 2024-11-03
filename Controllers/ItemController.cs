using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ItemHub.Interfaces;

namespace ItemHub.Controllers;

public class ItemController(IItemService itemService) : Controller
{

    [Route("/item")]
    public async Task<ActionResult> ViewItem(Guid id)
    {
        var item = await itemService.GetItemNoTracking(id);
        return item == null
            ? BadResult()
            : View(item);
    }

    [Route("/create")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public ActionResult Create() => View();

    [HttpPost] [Route("/create")] [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public async Task<ActionResult> Create(ItemModel model)
    {
        if(!ModelState.IsValid) return View();
        var error = await itemService.CreateAsync(model);
        return error != null
            ? BadResult(error)
            : RedirectToAction("Index", "Home");
    }
        
    [Route("/edit")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public async Task<ActionResult> Edit(Guid id)
    {
        var item = await itemService.GetItemNoTracking(id);
        if (item == null) return BadRequest("Такого товара не существует :(");
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator 
            ? RedirectToAction("ViewItem", "Item", new { id })
            : View(item);
    }

    [HttpPut]
    [Route("/update")]
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


    #region OldApi

    
    
    // [HttpPost]
    // [Authorize(Roles = UserRoles.CUSTOMER)]
    // [Route("FavoritedItems")]
    // public async Task<IActionResult> FavoritedItems([FromBody] ItemGuidRequest request)
    // {
    //     var user = await userRepository.GetUser();
    //     if (user == null) return BadRequest("Вы не вошли в аккаунт");
    //     if (!user.FavoritedItemsId.Remove(request.Id))
    //         user.FavoritedItemsId.Add(request.Id);
    //     await userRepository.UpdateUser(user);
    //     return Ok();
    // }
    //
    //     
    // [HttpPost]
    // [Authorize(Roles = UserRoles.CUSTOMER)]
    // [Route("favcount")]
    // public async Task<int> FavoritedItemsCount()
    // {
    //     var user = await userRepository.GetUser();
    //     return user != null ? user.FavoritedItemsId.Count : 0;
    // }
    //     
    //     
    // [HttpPost]
    // [Authorize(Roles = UserRoles.CUSTOMER)]
    // [Route("GetFavoritedItems")]
    // public async Task<List<Guid>> GetFavoritedItems()
    // {
    //     var user = await userRepository.GetUser();
    //     if (user == null) return [];
    //     var validItems = new List<Guid>();
    //
    //     foreach (var id in user.FavoritedItemsId.ToList())
    //     {
    //         var item = await itemRepository.GetItemNoTracking(id);
    //         if (item != null) validItems.Add(id);
    //         else user.FavoritedItemsId.Remove(id);
    //     }
    //     await userRepository.UpdateUser(user); 
    //     return validItems;
    // }
    //
    //
    //
    // [HttpPost]
    // [Route("PublishedItem")]
    // public async Task<IActionResult> PublishedItem([FromBody] ItemGuidRequest request)
    // {
    //     var item = await itemRepository.GetItem(request.Id);
    //     if (item == null) return BadRequest("Такого товара не существует.");
    //     if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
    //     {
    //         return BadRequest("Это не ваш товар.");
    //     }
    //     item.Published = !item.Published;
    //     await itemRepository.UpdateItem(item);
    //     return Ok(!item.Published);
    // }
    //     
    // [HttpDelete("/delete-image")]
    // public IActionResult DeleteImage(string login, string id, string fileName)
    // {
    //     var filePath = Path.Combine("wwwroot/images", login, id, fileName);
    //     if (!System.IO.File.Exists(filePath)) return BadRequest("Не удалось найти файл");
    //     System.IO.File.Delete(filePath);
    //     return Ok();
    // }
    //     
    // [HttpGet("/get-saved-images")]
    // public IActionResult GetSavedImages(string login, string id)
    // {
    //     var imageFiles = Directory.GetFiles(Path.Combine(_webRootPath, "images", login, id))
    //         .Select(Path.GetFileName)
    //         .ToList();
    //     var images = imageFiles.Select(fileName => new { fileName }).ToList();
    //     return Json(images);
    // }

    #endregion

    private ActionResult BadResult(string error = "Что-то пошло не так :(") => BadRequest(error);
}
// public class ItemGuidRequest
// {
//     public Guid Id { get; init; }
// }
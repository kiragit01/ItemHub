using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ItemHub.Repository.Interfaces;
using ItemHub.Utilities;

namespace ItemHub.Controllers
{
    public class ItemController(IItemDb dbI, IUserDb dbU, IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly string _webRootPath = webHostEnvironment.WebRootPath;

        // GET: ItemController
        [Route("/item")]
        public async Task<ActionResult> ViewItem(Guid id)
        {
            var item = await dbI.GetItemNoTracking(id);
            if (item == null) return BadRequest("Что-то пошло не так :(");
            ViewBag.Item = item;
            return View();
        }


        // GET: ItemController/Create
        [Route("/create")]
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ItemController/Create
        [Route("/create")]
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (user == null)   return BadRequest("Вы не вошли в аккаунт");
                var id = Guid.NewGuid();
                var pathImages = await UploadFiles.UploadItemImages(model.Images, user.Login, _webRootPath, id);
                
                Item item = new(id, model.Title, model.Description, user.Login, pathImages, model.Price, !model.Published);

                user.CustomItems.Add(item);
                try
                {
                    await dbI.AddItem(item);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict("Item не сохраняется в таблице из-за чего не может быть связан с пользователем");
                }
            }
            else
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }




        // GET: ItemController/Edit/5
        [Route("/edit")]
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var item = await dbI.GetItemNoTracking(id);
            if (item == null) return BadRequest("Такого товара не существует :(");
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
            {
                return RedirectToAction("ViewItem", "Item", new { id });
            }
            ViewBag.Item = item;
            return View();
        }

        // POST: ItemController/Edit/5
        [Route("/edit")]
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, ItemModel model, List<string> savedImages)
        {
            if (!ModelState.IsValid) return View();
            var itemDb = await dbI.GetItem(id);
            if (itemDb == null) return BadRequest("Такого товара не существует :(");
            
            // Обработка новых изображений
            var pathImages = await UploadFiles.UploadItemImages(model.Images, itemDb.Creator, _webRootPath, id);
            pathImages.AddRange(savedImages);
            if(pathImages.Count != 1 && pathImages[0] == "images/NoImage.png") pathImages.RemoveAt(0);
            
            itemDb.PathImages = pathImages;
            itemDb.Title = model.Title;
            itemDb.Description = model.Description;
            itemDb.Price = model.Price;
            itemDb.UpdatedDate = DateTime.UtcNow;
            
            await dbI.UpdateItem(itemDb);
            return RedirectToAction("ViewItem", "Item", new { id });
        }

        // POST: ItemController/Delete/5
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await dbI.GetItem(id);
            if (item == null) return BadRequest("Такого товара не существует :(");
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
            {
                return RedirectToAction("Index","Home");
            }
            await dbI.RemoveItem(item);
            return RedirectToAction("Index","Home");
        }


        [HttpPost]
        [Authorize(Roles = UserRoles.CUSTOMER)]
        [Route("FavoritedItems")]
        public async Task<IActionResult> FavoritedItems([FromBody] ItemGuidRequest request)
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null) return BadRequest("Вы не вошли в аккаунт");
            if (!user.FavoritedItemsId.Remove(request.Id))
                user.FavoritedItemsId.Add(request.Id);
            await dbU.UpdateUser(user);
            return Ok();
        }

        
        [HttpPost]
        [Authorize(Roles = UserRoles.CUSTOMER)]
        [Route("favcount")]
        public async Task<int> FavoritedItemsCount()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return user != null ? user.FavoritedItemsId.Count : 0;
        }
        
        
        [HttpPost]
        [Authorize(Roles = UserRoles.CUSTOMER)]
        [Route("GetFavoritedItems")]
        public async Task<List<Guid>> GetFavoritedItems()
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user == null) return [];
            var validItems = new List<Guid>();

            foreach (var id in user.FavoritedItemsId.ToList())
            {
                var item = await dbI.GetItemNoTracking(id);
                if (item != null) validItems.Add(id);
                else user.FavoritedItemsId.Remove(id);
            }
            await dbU.UpdateUser(user); 
            return validItems;
        }



        [HttpPost]
        [Route("PublishedItem")]
        public async Task<IActionResult> PublishedItem([FromBody] ItemGuidRequest request)
        {
            var item = await dbI.GetItem(request.Id);
            if (item == null) return BadRequest("Такого товара не существует.");
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
            {
                return BadRequest("Это не ваш товар.");
            }
            item.Published = !item.Published;
            await dbI.UpdateItem(item);
            return Ok(!item.Published);
        }
        
        [HttpDelete("/delete-image")]
        public IActionResult DeleteImage(string login, string id, string fileName)
        {
            var filePath = Path.Combine("wwwroot/images", login, id, fileName);
            if (!System.IO.File.Exists(filePath)) return BadRequest("Не удалось найти файл");
            System.IO.File.Delete(filePath);
            return Ok();
        }
        
        [HttpGet("/get-saved-images")]
        public IActionResult GetSavedImages(string login, string id)
        {
            var imageFiles = Directory.GetFiles(Path.Combine(_webRootPath, "images", login, id))
                .Select(Path.GetFileName)
                .ToList();
            var images = imageFiles.Select(fileName => new { fileName }).ToList();
            return Json(images);
        }
    }
    public class ItemGuidRequest
    {
        public Guid Id { get; init; }
    }
}

using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ItemHub.Repository.Interfaces;

namespace ItemHub.Controllers
{
    public class ItemController(IItemDb dbI, IUserDb dbU, IWebHostEnvironment webHostEnvironment) : Controller
    {


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
                var pathImages = await UploadImages(model.Images, user.Login, id);
                
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
            var pathImages = await UploadImages(model.Images, itemDb.Creator, id);
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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.CUSTOMER)]
        public async Task<IActionResult> FavoritedItems(Guid id)
        {
            var user = await dbU.GetUser(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null) return BadRequest("Вы не вошли в аккаунт");
            if (!user.FavoritedItemsId.Remove(id))
                user.FavoritedItemsId.Add(id);
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
            if (user == null) return new List<Guid>();
            var favoritedId = user.FavoritedItemsId;
            var favoritedItems = new List<Guid>();
            var deleteFavorited = new List<Guid>();
            foreach (var id in favoritedId)
            {
                if (await dbI.GetItemNoTracking(id) != null) 
                    favoritedItems.Add(id);
                else deleteFavorited.Add(id);
            }
            foreach (var id in deleteFavorited)
            {
                user.FavoritedItemsId.Remove(id);
            }
            return favoritedItems;
        }



        [HttpPost]
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        public async Task<IActionResult> PublishedItem(Guid id)
        {
            var item = await dbI.GetItem(id);
            if (item == null) return BadRequest("Такого товара не существует :(");
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
            {
                return BadRequest("Это не ваш товар.");
            }
            item.Published = !item.Published;
            await dbI.UpdateItem(item);
            return Ok();
        }
        

        //Генератор рандомного набора символов
        private static readonly Random Random = new();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
        
        //Создание путей к фото товара
        private async Task<List<string>> UploadImages(IFormFileCollection? files, string userLogin, Guid id)
        {
            if (files == null) return ["images/NoImage.png"];
            
            var pathImages = new List<string>();
            var uploadPath = Path.Combine(webHostEnvironment.WebRootPath, "images", userLogin, id.ToString());
            // создаем папку для хранения файлов
            Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var type = "." + file.ContentType.Split("/")[1];
                // путь к файлу
                var pathForList = Path.Combine("images", userLogin, id.ToString(), RandomString(8) + type);
                var fullPath = Path.Combine(webHostEnvironment.WebRootPath, pathForList);
                pathImages.Add(pathForList);
                // сохраняем файл
                var fileStream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(fileStream);
            }
            return pathImages;
        }

        [HttpDelete("/delete-image")]
        public IActionResult DeleteImage(string login, string id, string fileName)
        {
            var filePath = Path.Combine("wwwroot/images", login, id, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Ok();
            }
            return BadRequest("Не удалось найти файл");
        }
        
        [HttpGet("/get-saved-images")]
        public IActionResult GetSavedImages(string login, string id)
        {
            var imageFiles = Directory.GetFiles(Path.Combine(webHostEnvironment.WebRootPath, "images", login, id))
                .Select(Path.GetFileName)
                .ToList();

            var images = imageFiles.Select(fileName => new { fileName }).ToList();
            return Json(images);
        }
    }
}

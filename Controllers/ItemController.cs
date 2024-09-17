using ItemHub.Data;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using System.Security.Claims;

namespace ItemHub.Controllers
{
    public class ItemController(UserContext db, IWebHostEnvironment webHostEnvironment) : Controller
    {


        // GET: ItemController
        [Route("/item")]
        public async Task<ActionResult> ViewItem(Guid id)
        {
            Item? item = await db.Items.FirstOrDefaultAsync(o => o.Id == id);
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
                var userLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userLogin == null)  return Unauthorized();
                var user = await db.Users
                           .Include(u => u.Items)
                           .FirstOrDefaultAsync(o => o.Login == userLogin);
                if (user == null)   return NotFound();
                var id = Guid.NewGuid();
                List<string> pathImages = await UploadImages(model.Images, user.Login, id);
                
                Item item = new(id, model.Title, model.Description, user.Login, pathImages, model.Price);

                user.Items.Add(item);
                await db.Items.AddAsync(item);
                try
                {
                    await db.SaveChangesAsync();
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
            Item? item = await db.Items.FirstOrDefaultAsync(o => o.Id == id);
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
            Item? itemDb = await db.Items.FirstOrDefaultAsync(o => o.Id == id);
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
            
            db.Items.Update(itemDb);
            await db.SaveChangesAsync();
            
            return RedirectToAction("ViewItem", "Item", new { id });
        }

        // POST: ItemController/Delete/5
        [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            Item? item = await db.Items.FirstOrDefaultAsync(o => o.Id == id);
            if (item == null) return BadRequest("Такого товара не существует :(");
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != item.Creator)
            {
                return RedirectToAction("Index","Home");
            }

            db.Items.Remove(item);
            await db.SaveChangesAsync();
            
            return RedirectToAction("Index","Home");
        }
        
        
        [HttpPost]
        public IActionResult SavedItems(Guid id)
        {
            // Получаем список сохранённых ID товаров
            Console.WriteLine(id);

            // Вернём успешный ответ
            return Ok();
        }
        
        

        //Генератор рандомного набора символов
        private static readonly Random random = new();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
                await using var fileStream = new FileStream(fullPath, FileMode.Create);
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

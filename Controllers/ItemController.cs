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
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public class ItemController : Controller
    {

        private readonly UserContext db;
        private string? UserLogin;
        public ItemController(UserContext Dbcontext)
        {
            db = Dbcontext;
        }



        #region Temp
        // GET: ItemController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ItemController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
        #endregion

        // GET: ItemController/Create
        [Route("/1")]
        public ActionResult Create()
        {
            UserLogin = User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Actor).Value.ToString();
            return View();
        }


        // POST: ItemController/Create
        [Route("/1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemModel model)
        {
            if(ModelState.IsValid)
            {
                List<string> PathImages = UploadImages(model.Images).Result;
                Item item = new(model.Title, model.Description, PathImages, model.Price);
                db.Items.Add(item);
                await db.SaveChangesAsync();
            }
            return View();
        }


        public async Task<List<string>> UploadImages(IFormFileCollection files)
        {
            List<string> PathImages = new();
            if (files != null)
            {
                var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/img/{UserLogin}";
                // создаем папку для хранения файлов
                Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    var type = "." + file.ContentType.Split("/")[1];
                    // путь к файлу
                    string fullPath = $"{uploadPath}/{RandomString(8) + type}";
                    PathImages.Add(fullPath);
                    // сохраняем файл
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                return PathImages;
            }
            else
            {
                return [$"{Directory.GetCurrentDirectory()}/wwwroot/img/NoImage.png"];
            }

        }





        // GET: ItemController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ItemController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ItemController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ItemController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        //Генератор рандомного набора символов
        private readonly static Random random = new();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

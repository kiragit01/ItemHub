﻿using ItemHub.Data;
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
        //private User? user;
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



        public async Task<List<Item>> ListItems() => await db.Items.ToListAsync();



        // GET: ItemController/Create
        [Route("/create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ItemController/Create
        [Route("/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemModel model)
        {
            if(ModelState.IsValid)
            {
                var userLogin = User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.NameIdentifier).Value;
                if (userLogin == null)  return Unauthorized();
                var user = await db.Users
                           .Include(u => u.Items)
                           .FirstOrDefaultAsync(o => o.Login == userLogin);
                if (user == null)   return NotFound();

                List<string> PathImages = await UploadImages(model.Images, user);

                Item item = new(model.Title, model.Description, PathImages, model.Price);

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

        [NonAction]
        public async Task<List<string>> UploadImages(IFormFileCollection files, User user)
        {
            var pathImages = new List<string>();
            if (files != null)
            {
                var uploadPath = $"images/{user.Login}";
                // создаем папку для хранения файлов
                Directory.CreateDirectory("wwwroot/"+uploadPath);

                foreach (var file in files)
                {
                    var type = "." + file.ContentType.Split("/")[1];
                    // путь к файлу
                    string pathForList = $"{uploadPath}/{RandomString(8) + type}";
                    string fullPath = $"wwwroot/{pathForList}";
                    pathImages.Add(pathForList);
                    // сохраняем файл
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                return pathImages;
            }
            else
            {
                return [$"images/NoImage.png"];
            }

        }
            //else
            //{
            //    return [$"{Directory.GetCurrentDirectory()}/wwwroot/img/NoImage.png"];
            //}



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

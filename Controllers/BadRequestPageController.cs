using Microsoft.AspNetCore.Mvc;

namespace ItemHub.Controllers
{
    public class BadRequestPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

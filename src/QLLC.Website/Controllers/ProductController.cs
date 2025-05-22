using Microsoft.AspNetCore.Mvc;

namespace Tasin.Website.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Tasin.Website.Controllers
{
    public class VendorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

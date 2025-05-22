using Microsoft.AspNetCore.Mvc;

namespace Tasin.Website.Controllers
{
    public class UnitController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

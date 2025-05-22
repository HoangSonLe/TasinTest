using Microsoft.AspNetCore.Mvc;

namespace Tasin.Website.Controllers
{
    public class PurchaseOrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

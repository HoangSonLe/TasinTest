using Microsoft.AspNetCore.Mvc;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;

namespace Tasin.Website.Controllers
{
    public class ProvincesController :  BaseController<ProvincesController>
    {
        private readonly IProvincesService _provincesService;
        public ProvincesController(IUserService userService, IProvincesService tenantService,
            ILogger<ProvincesController> logger) : base(logger, userService)
        {
            _provincesService = tenantService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetListProvince()
        {
            var result = await _provincesService.GetUserDataDropdownList("");
            return Json(result.Data);
        }
    }
}

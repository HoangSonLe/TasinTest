using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;

namespace Tasin.Website.Controllers
{
    //[Authorize]
    [ApiController]
    public class CategoryController : BaseController<CategoryController>
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(
            ILogger<CategoryController> logger,
            IUserService userService,
            ICategoryService categoryService,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        [Route("Category/GetDataOptionsDropdown")]
        public async Task<IActionResult> GetDataOptionsDropdown(string? searchString, ECategoryType type)
        {
            var result = await _categoryService.GetDataOptionsDropdown(searchString,type);
            return Json(result);
        }
    }
}

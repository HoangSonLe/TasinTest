using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;

namespace Tasin.Website.Controllers
{
    //[Authorize]
    public class RoleController : BaseController<RoleController>
    {
        private readonly IRoleService _roleService;

        public RoleController(
            ILogger<RoleController> logger,
            IUserService userService,
            IRoleService roleService,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _roleService = roleService;
        }
        public async Task<IActionResult> GetRoleDropdownList(string searchString)
        {
            var result = await _roleService.GetRoleDropdownList(searchString);
            return Json(result);
        }
    }
}

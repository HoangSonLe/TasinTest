using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;

namespace Tasin.Website.Controllers
{
    [Authorize]
    [ApiController]
    public class CommonController : BaseController<CommonController>
    {
        private readonly ICommonService _commonService;

        public CommonController(
            ILogger<CommonController> logger,
            IUserService userService,
            ICommonService commonService,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _commonService = commonService;
        }
        [HttpGet]
        [Route("Common/GetDataOptionsDropdown")]
        public async Task<IActionResult> GetDataOptionsDropdown(string? searchString, ECategoryType type)
        {
            var result = await _commonService.GetDataOptionsDropdown(searchString,type);
            return Json(result);
        }
    }
}

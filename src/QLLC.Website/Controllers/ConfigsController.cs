using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    public class ConfigsController : BaseController<ConfigsController>
    {
        private readonly IConfigService _configService;
       
        public ConfigsController(IUserService userService, IConfigService configService,
           ILogger<ConfigsController> logger) : base(logger, userService)
        {
            _configService = configService;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CONFIG])]
        public IActionResult Index()
        {
            return View();
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CONFIG])]
        public async Task<IActionResult> GetConfigList(UserSearchModel searchModel)
        {

            var result = await _configService.GetConfigList(searchModel);
            return Json(result);
        }
        [HttpPost]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_CONFIG])]
        public async Task<Acknowledgement> CreateOrUpdate([FromBody] ConfigViewModel postData)
        {
            return await _configService.CreateOrUpdate(postData);
        }
    }
}

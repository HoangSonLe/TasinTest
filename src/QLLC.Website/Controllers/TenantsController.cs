using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class TenantsController : BaseController<TenantsController>
    {
        private readonly ITenantService _tenantService;
        public TenantsController(IUserService userService, ITenantService tenantService,
            ILogger<TenantsController> logger) : base(logger, userService)
        {
            _tenantService = tenantService;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_TENT])]
        public IActionResult Index()
        {
            return View();
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_TENT])]
        public async Task<IActionResult> GetTenantList(UserSearchModel searchModel)
        {

            var result = await _tenantService.GetTenantList(searchModel);
            return Json(result);
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_TENT])]
        public async Task<Acknowledgement> DeleteTenantById(int tenantId)
        {
            return await _tenantService.DeleteTenantById(tenantId);

        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_TENT, (int)EActionRole.UPDATE_TENT])]
        [HttpPost]
        public async Task<Acknowledgement> CreateOrUpdate([FromBody] TenantViewModel postData)
        {
            return await _tenantService.CreateOrUpdate(postData);
        }
    }
}

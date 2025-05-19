using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Tasin.Website.DAL.Services.WebInterfaces;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class HistoryLoginController : BaseController<HistoryLoginController>
    {
        private readonly IHistoryLoginService _historyLoginService;
        private readonly IConfiguration _configuration;
        public HistoryLoginController(
            IUserService userService,
            IHistoryLoginService historyLoginService,
             ILogger<HistoryLoginController> logger,
             IConfiguration configuration
            ) : base(logger, userService)
        {
            _historyLoginService = historyLoginService;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_STORAGE])]
        public async Task<Acknowledgement<JsonResultPaging<List<HistoryLoginViewModel>>>> GetHistoryLoginList(HistoryLoginSearchModel searchModel)
        {
            return await _historyLoginService.GetHistoryLoginList(searchModel);
        }
    }
}

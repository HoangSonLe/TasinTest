using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using System.Data.Entity;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class ReminderController : BaseController<ReminderController>
    {
        private readonly IReminderService _reminderService;
        public ReminderController(
            IUserService userService,
            IReminderService reminderService,
             ILogger<ReminderController> logger
            ) : base(logger, userService)
        {
            _reminderService = reminderService;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_REMINDER])]
        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        public async Task<Acknowledgement<JsonResultPaging<List<ReminderViewModel>>>> GetReminderList(ReminderSearchModel searchModel)
        {
            try
            {

                return await _reminderService.GetReminderList(searchModel);
            }
            catch (Exception ex)
            {
                return new Acknowledgement<JsonResultPaging<List<ReminderViewModel>>>();
            }
        }
        public async Task<Acknowledgement<ReminderViewModel>> GetReminderById(long reminderId)
        {
            return await _reminderService.GetReminderById(reminderId);
        }

        [HttpPost]
        public async Task<Acknowledgement> CreateOrUpdateReminder([FromBody]ReminderViewModel postData)
        {
            return await _reminderService.CreateOrUpdateReminder(postData);
        }
        [HttpPost]
        public async Task<Acknowledgement> DeleteReminderById(long reminderId)
        {
            return await _reminderService.DeleteReminderById(reminderId);

        }





    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class UserController : BaseController<UserController>
    {
        public UserController(IUserService userService,
             ILogger<UserController> logger) : base(logger, userService)
        {
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        public IActionResult Index()
        {
            ViewBag.RoleDatasource = EnumHelper.ToDropdownList<ERoleType>();
            return View();
        }
        public async Task<IActionResult> Authentication()
        {
            var response = new Acknowledgement();
            if(User.Identity.IsAuthenticated)
            {
                var userAck = await UserService.GetUserById(Int32.Parse(_currentUserId));
                return Json(userAck);
            }
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetUserDropdownList(string searchString,string selectedIdList)
        {
            var selectedIds = selectedIdList?.Split(',').Select(int.Parse).ToList();
            var result = await UserService.GetUserDataDropdownList(searchString, selectedIds ?? new List<int>());
            return Json(result);
        }
        [HttpGet]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        public async Task<IActionResult> GetUserList([FromQuery]UserSearchModel searchModel)
        {

            var result = await UserService.GetUserList(searchModel);
            return Json(result);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_USER])]
        [HttpGet]
        public async Task<Acknowledgement> DeleteUserById(int userId)
        {
            return await UserService.DeleteUserById(userId);
        } 
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [HttpGet]
        public async Task<Acknowledgement> ResetUserPasswordById(int userId)
        {
            return await UserService.ResetUserPasswordById(userId);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_USER, (int)EActionRole.UPDATE_USER])]
        [HttpPost]
        public async Task<Acknowledgement> CreateOrUpdateUser([FromBody] UserViewModel postData)
        {
            return await UserService.CreateOrUpdateUser(postData);
        }      
        [HttpPost]
        public async Task<Acknowledgement> ChangePassword([FromBody] ChangePasswordModel postData)
        {
            return await UserService.ChangePassword(postData);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER, (int)EActionRole.UPDATE_USER])]
        [HttpGet]
        public async Task<Acknowledgement<UserViewModel>> GetUserById(int userId)
        {
            var ack = await UserService.GetUserById(userId);
            return ack;
        }
        public string Values()
        {
            return "Server is running";
        }


    }
}

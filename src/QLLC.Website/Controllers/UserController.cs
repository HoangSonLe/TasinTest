using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    public class UserController : BaseController<UserController>
    {
        public UserController(
            IUserService userService,
            ILogger<UserController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [HttpGet]
        [Route("User/Index")]
        public IActionResult Index()
        {
            ViewBag.RoleDatasource = EnumHelper.ToDropdownList<ERoleType>();
            return View();
        }
        [HttpGet]
        [Route("User/Authentication")]
        public async Task<IActionResult> Authentication()
        {
            var response = new Acknowledgement();
            if(CurrentUserContext.IsAuthenticated && CurrentUserContext.UserId.HasValue)
            {
                var userAck = await UserService.GetUserById(CurrentUserContext.UserId.Value);
                return Json(userAck);
            }
            return Json(response);
        }
        [HttpGet]
        [Route("User/GetUserDropdownList")]
        public async Task<IActionResult> GetUserDropdownList(string searchString,string selectedIdList)
        {
            var selectedIds = selectedIdList?.Split(',').Select(int.Parse).ToList();
            var result = await UserService.GetUserDataDropdownList(searchString, selectedIds ?? new List<int>());
            return Json(result);
        }
        /// <summary>
        /// Get a list of users with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet]
        [AllowAnonymous] // Tạm thời cho phép truy cập không cần xác thực
        // [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<UserViewModel>>>), 200)]
        [Produces("application/json")]
        [Route("api/[controller]/GetUserList")]
        public async Task<IActionResult> GetUserList([FromQuery]UserSearchModel searchModel)
        {
            var result = await UserService.GetUserList(searchModel);
            return Json(result);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_USER])]
        [HttpGet]
        [Route("User/DeleteUserById")]
        public async Task<Acknowledgement> DeleteUserById(int userId)
        {
            return await UserService.DeleteUserById(userId);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [HttpGet]
        [Route("User/ResetUserPasswordById")]
        public async Task<Acknowledgement> ResetUserPasswordById(int userId)
        {
            return await UserService.ResetUserPasswordById(userId);
        }
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_USER, (int)EActionRole.UPDATE_USER])]
        [HttpPost]
        [Route("User/CreateOrUpdateUser")]
        public async Task<Acknowledgement> CreateOrUpdateUser([FromBody] UserViewModel postData)
        {
            return await UserService.CreateOrUpdateUser(postData);
        }
        [HttpPost]
        [Route("User/ChangePassword")]
        public async Task<Acknowledgement> ChangePassword([FromBody] ChangePasswordModel postData)
        {
            return await UserService.ChangePassword(postData);
        }
        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User details</returns>
        /// <response code="200">Returns the user details</response>
        /// <response code="404">If the user is not found</response>
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER, (int)EActionRole.UPDATE_USER])]
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<UserViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("api/[controller]/GetUserById/{userId}")]
        public async Task<Acknowledgement<UserViewModel>> GetUserById(int userId)
        {
            var ack = await UserService.GetUserById(userId);
            return ack;
        }
        [HttpGet]
        [Route("User/Values")]
        public string Values()
        {
            return "Server is running";
        }


    }
}

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
    [ApiController]
    [Produces("application/json")]
    public class UserController : BaseController<UserController>
    {
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public UserController(
            IUserService userService,
            ICommonService commonService,
            ILogger<UserController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _userService = userService;
            _commonService = commonService;
        }

        /// <summary>
        /// User management page
        /// </summary>
        /// <returns>User management view</returns>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [HttpGet]
        [Route("User/Index")]
        public IActionResult Index()
        {
            ViewBag.RoleDatasource = EnumHelper.ToDropdownList<ERoleType>();
            return View();
        }

        /// <summary>
        /// Get current user authentication status
        /// </summary>
        /// <returns>Current user information if authenticated</returns>
        [HttpGet]
        [Route("User/Authentication")]
        public async Task<IActionResult> Authentication()
        {
            var response = new Acknowledgement();
            if (CurrentUserContext.IsAuthenticated && CurrentUserContext.UserId.HasValue)
            {
                var userAck = await _userService.GetUserById(CurrentUserContext.UserId.Value);
                return Json(userAck);
            }
            return Json(response);
        }

        /// <summary>
        /// Get a list of users with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet]
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<UserViewModel>>>), 200)]
        [Route("User/GetUserList")]
        public async Task<IActionResult> GetUserList([FromQuery] UserSearchModel searchModel)
        {
            var result = await _userService.GetUserList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Delete a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Result of the operation</returns>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_USER])]
        [HttpDelete]
        [Route("User/DeleteUserById")]
        public async Task<Acknowledgement> DeleteUserById(int userId)
        {
            return await _userService.DeleteUserById(userId);
        }

        /// <summary>
        /// Reset a user's password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Result of the operation</returns>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [HttpPost]
        [Route("User/ResetUserPasswordById")]
        public async Task<Acknowledgement> ResetUserPasswordById(int userId)
        {
            return await _userService.ResetUserPasswordById(userId);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="postData">User data</param>
        /// <returns>Result of the operation</returns>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_USER])]
        [HttpPost]
        [Route("User/Create")]
        public async Task<Acknowledgement> Create([FromBody] UserViewModel postData)
        {
            return await _userService.CreateOrUpdateUser(postData);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="postData">Updated user data</param>
        /// <returns>Result of the operation</returns>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [HttpPut]
        [Route("User/UpdateUser/{userId}")]
        public async Task<Acknowledgement> UpdateUser([FromRoute] int userId, [FromBody] UserViewModel postData)
        {
            postData.Id = userId;
            return await _userService.CreateOrUpdateUser(postData);
        }

        /// <summary>
        /// Change a user's password
        /// </summary>
        /// <param name="postData">Password change data</param>
        /// <returns>Result of the operation</returns>
        [HttpPost]
        [Route("User/ChangePassword")]
        public async Task<Acknowledgement> ChangePassword([FromBody] ChangePasswordModel postData)
        {
            return await _userService.ChangePassword(postData);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User details</returns>
        /// <response code="200">Returns the user details</response>
        /// <response code="404">If the user is not found</response>
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER, (int)EActionRole.UPDATE_USER])]
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<UserViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("User/GetUserById/{userId}")]
        public async Task<Acknowledgement<UserViewModel>> GetUserById(int userId)
        {
            return await _userService.GetUserById(userId);
        }
       
    }
}

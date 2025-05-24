using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// API Controller for managing users
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UserApiController : BaseController<UserApiController>
    {
        private readonly IUserService _userService;

        public UserApiController(
            IUserService userService,
            ILogger<UserApiController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get a list of users with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<UserViewModel>>>), 200)]
        [Route("GetUserList")]
        public async Task<IActionResult> GetUserList([FromQuery] UserSearchModel searchModel)
        {
            var result = await _userService.GetUserList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User details</returns>
        /// <response code="200">Returns the user details</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [ProducesResponseType(typeof(Acknowledgement<UserViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("GetUserById/{userId}")]
        public async Task<Acknowledgement<UserViewModel>> GetUserById(int userId)
        {
            return await _userService.GetUserById(userId);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="postData">User data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_USER])]
        [Route("Create")]
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
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [Route("UpdateUser/{userId}")]
        public async Task<Acknowledgement> UpdateUser([FromRoute] int userId, [FromBody] UserViewModel postData)
        {
            postData.Id = userId;
            return await _userService.CreateOrUpdateUser(postData);
        }

        /// <summary>
        /// Delete a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpDelete]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_USER])]
        [Route("DeleteUserById")]
        public async Task<Acknowledgement> DeleteUserById(int userId)
        {
            return await _userService.DeleteUserById(userId);
        }

        /// <summary>
        /// Reset a user's password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_USER])]
        [Route("ResetUserPasswordById")]
        public async Task<Acknowledgement> ResetUserPasswordById(int userId)
        {
            return await _userService.ResetUserPasswordById(userId);
        }

        /// <summary>
        /// Change a user's password
        /// </summary>
        /// <param name="postData">Password change data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<Acknowledgement> ChangePassword([FromBody] ChangePasswordModel postData)
        {
            return await _userService.ChangePassword(postData);
        }

        /// <summary>
        /// Get user dropdown list for selection
        /// </summary>
        /// <param name="searchString">Search string to filter users</param>
        /// <param name="selectedIdList">Comma-separated list of selected user IDs</param>
        /// <returns>List of users for dropdown</returns>
        [HttpGet]
        [Route("GetUserDropdownList")]
        public async Task<IActionResult> GetUserDropdownList(string searchString, string selectedIdList)
        {
            var selectedIds = selectedIdList?.Split(',').Select(int.Parse).ToList();
            var result = await _userService.GetUserDataDropdownList(searchString, selectedIds ?? new List<int>());
            return Json(result);
        }
    }
}

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

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// </summary>
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    public class CategoryController : BaseController<CategoryController>
    {
        private ICategoryService _categoryService;
        public CategoryController(
            ICategoryService categoryService,
            IUserService userService,
            ILogger<CategoryController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Route("Category/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CATEGORY])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of categories with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of categories</returns>
        /// <response code="200">Returns the list of categories</response>
        [HttpGet]
        [Route("Category/GetCategoryList")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CATEGORY])]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<CategoryViewModel>>>), 200)]
        public async Task<IActionResult> GetCategoryList([FromQuery] CategorySearchModel searchModel)
        {
            var result = await _categoryService.GetCategoryList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("Category/DeleteCategoryById/{categoryId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_CATEGORY])]
        public async Task<Acknowledgement> DeleteCategoryById([FromRoute] int categoryId)
        {
            return await _categoryService.DeleteCategoryById(categoryId);
        }

        [HttpPost]
        [Route("Category/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_CATEGORY])]
        public async Task<Acknowledgement> Create([FromBody] CategoryViewModel postData)
        {
            return await _categoryService.CreateOrUpdateCategory(postData);
        }

        [HttpPut]
        [Route("Category/UpdateCategory/{categoryId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_CATEGORY])]
        public async Task<Acknowledgement> CreateOrUpdateCategory([FromRoute] int categoryId, [FromBody] CategoryViewModel postData)
        {
            postData.Id = categoryId;
            return await _categoryService.CreateOrUpdateCategory(postData);
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Category details</returns>
        /// <response code="200">Returns the category details</response>
        /// <response code="404">If the category is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<CategoryViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("Category/GetCategoryById/{categoryId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CATEGORY])]
        public async Task<Acknowledgement<CategoryViewModel>> GetCategoryById(int categoryId)
        {
            var ack = await _categoryService.GetCategoryById(categoryId);
            return ack;
        }
    }
}

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
    /// Controller for managing materials
    /// </summary>
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    public class MaterialController : BaseController<MaterialController>
    {
        private IMaterialService _materialService;
        public MaterialController(
            IMaterialService materialService,
            IUserService userService,
            ILogger<MaterialController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _materialService = materialService;
        }

        [HttpGet]
        [Route("Material/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_MATERIAL])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of materials with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of materials</returns>
        /// <response code="200">Returns the list of materials</response>
        [HttpGet]
        [Route("Material/GetMaterialList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<MaterialViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_MATERIAL])]
        public async Task<IActionResult> GetMaterialList([FromQuery] MaterialSearchModel searchModel)
        {
            var result = await _materialService.GetMaterialList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("Material/DeleteMaterialById/{materialId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_MATERIAL])]
        public async Task<Acknowledgement> DeleteMaterialById([FromRoute] int materialId)
        {
            return await _materialService.DeleteMaterialById(materialId);
        }

        [HttpPost]
        [Route("Material/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_MATERIAL])]
        public async Task<Acknowledgement> Create([FromBody] MaterialViewModel postData)
        {
            return await _materialService.CreateOrUpdateMaterial(postData);
        }

        [HttpPut]
        [Route("Material/UpdateMaterial/{materialId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_MATERIAL])]
        public async Task<Acknowledgement> CreateOrUpdateMaterial([FromRoute] int materialId, [FromBody] MaterialViewModel postData)
        {
            postData.Id = materialId;
            return await _materialService.CreateOrUpdateMaterial(postData);
        }

        /// <summary>
        /// Get a specific material by ID
        /// </summary>
        /// <param name="materialId">Material ID</param>
        /// <returns>Material details</returns>
        /// <response code="200">Returns the material details</response>
        /// <response code="404">If the material is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<MaterialViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("Material/GetMaterialById/{materialId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_MATERIAL])]
        public async Task<Acknowledgement<MaterialViewModel>> GetMaterialById(int materialId)
        {
            var ack = await _materialService.GetMaterialById(materialId);
            return ack;
        }
    }
}

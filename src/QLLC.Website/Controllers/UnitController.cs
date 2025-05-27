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
    /// Controller for managing units
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class UnitController : BaseController<UnitController>
    {
        private IUnitService _unitService;
        public UnitController(
            IUnitService unitService,
            IUserService userService,
            ILogger<UnitController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _unitService = unitService;
        }

        [HttpGet]
        [Route("Unit/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_UNIT])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of units with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of units</returns>
        /// <response code="200">Returns the list of units</response>
        [HttpGet]
        [Route("Unit/GetUnitList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<UnitViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_UNIT])]
        public async Task<IActionResult> GetUnitList([FromQuery] UnitSearchModel searchModel)
        {
            var result = await _unitService.GetUnitList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("Unit/DeleteUnitById/{unitId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_UNIT])]
        public async Task<Acknowledgement> DeleteUnitById([FromRoute] int unitId)
        {
            return await _unitService.DeleteUnitById(unitId);
        }

        [HttpPost]
        [Route("Unit/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_UNIT])]
        public async Task<Acknowledgement> Create([FromBody] UnitViewModel postData)
        {
            return await _unitService.CreateOrUpdateUnit(postData);
        }

        [HttpPut]
        [Route("Unit/UpdateUnit/{unitId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_UNIT])]
        public async Task<Acknowledgement> CreateOrUpdateUnit([FromRoute] int unitId, [FromBody] UnitViewModel postData)
        {
            postData.Id = unitId;
            return await _unitService.CreateOrUpdateUnit(postData);
        }

        /// <summary>
        /// Get a specific unit by ID
        /// </summary>
        /// <param name="unitId">Unit ID</param>
        /// <returns>Unit details</returns>
        /// <response code="200">Returns the unit details</response>
        /// <response code="404">If the unit is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<UnitViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("Unit/GetUnitById/{unitId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_UNIT, (int)EActionRole.UPDATE_UNIT])]
        public async Task<Acknowledgement<UnitViewModel>> GetUnitById(int unitId)
        {
            var ack = await _unitService.GetUnitById(unitId);
            return ack;
        }
    }
}

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
    /// Controller for managing special product tax rates
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class SpecialProductTaxRateController : BaseController<SpecialProductTaxRateController>
    {
        private ISpecialProductTaxRateService _specialProductTaxRateService;
        public SpecialProductTaxRateController(
            ISpecialProductTaxRateService specialProductTaxRateService,
            IUserService userService,
            ILogger<SpecialProductTaxRateController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _specialProductTaxRateService = specialProductTaxRateService;
        }

        [HttpGet]
        [Route("SpecialProductTaxRate/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_SPECIALPRODUCTTAXRATE])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of special product tax rates with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of special product tax rates</returns>
        /// <response code="200">Returns the list of special product tax rates</response>
        [HttpGet]
        [Route("SpecialProductTaxRate/GetSpecialProductTaxRateList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<SpecialProductTaxRateViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_SPECIALPRODUCTTAXRATE])]
        public async Task<IActionResult> GetSpecialProductTaxRateList([FromQuery] SpecialProductTaxRateSearchModel searchModel)
        {
            var result = await _specialProductTaxRateService.GetSpecialProductTaxRateList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("SpecialProductTaxRate/DeleteSpecialProductTaxRateById")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_SPECIALPRODUCTTAXRATE])]
        public async Task<Acknowledgement> DeleteSpecialProductTaxRateById(int specialProductTaxRateId)
        {
            return await _specialProductTaxRateService.DeleteSpecialProductTaxRateById(specialProductTaxRateId);
        }

        [HttpPost]
        [Route("SpecialProductTaxRate/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_SPECIALPRODUCTTAXRATE])]
        public async Task<Acknowledgement> Create([FromBody] SpecialProductTaxRateViewModel postData)
        {
            return await _specialProductTaxRateService.CreateOrUpdateSpecialProductTaxRate(postData);
        }

        [HttpPut]
        [Route("SpecialProductTaxRate/UpdateSpecialProductTaxRate/{specialProductTaxRateId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_SPECIALPRODUCTTAXRATE])]
        public async Task<Acknowledgement> CreateOrUpdateSpecialProductTaxRate([FromRoute] int specialProductTaxRateId, [FromBody] SpecialProductTaxRateViewModel postData)
        {
            postData.Id = specialProductTaxRateId;
            return await _specialProductTaxRateService.CreateOrUpdateSpecialProductTaxRate(postData);
        }

        /// <summary>
        /// Get a specific special product tax rate by ID
        /// </summary>
        /// <param name="specialProductTaxRateId">Special Product Tax Rate ID</param>
        /// <returns>Special product tax rate details</returns>
        /// <response code="200">Returns the special product tax rate details</response>
        /// <response code="404">If the special product tax rate is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<SpecialProductTaxRateViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("SpecialProductTaxRate/GetSpecialProductTaxRateById/{specialProductTaxRateId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_SPECIALPRODUCTTAXRATE, (int)EActionRole.UPDATE_SPECIALPRODUCTTAXRATE])]
        public async Task<Acknowledgement<SpecialProductTaxRateViewModel>> GetSpecialProductTaxRateById(int specialProductTaxRateId)
        {
            var ack = await _specialProductTaxRateService.GetSpecialProductTaxRateById(specialProductTaxRateId);
            return ack;
        }
    }
}

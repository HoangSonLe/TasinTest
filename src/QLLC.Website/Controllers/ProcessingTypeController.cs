using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing processing types
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class ProcessingTypeController : BaseController<ProcessingTypeController>
    {
        private IProcessingTypeService _processingTypeService;
        public ProcessingTypeController(
            IProcessingTypeService processingTypeService,
            IUserService userService,
            ILogger<ProcessingTypeController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _processingTypeService = processingTypeService;
        }

        [HttpGet]
        [Route("ProcessingType/Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of processing types with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of processing types</returns>
        /// <response code="200">Returns the list of processing types</response>
        [HttpGet]
        [Route("ProcessingType/GetProcessingTypeList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<ProcessingTypeViewModel>>>), 200)]
        public async Task<IActionResult> GetProcessingTypeList([FromQuery] ProcessingTypeSearchModel searchModel)
        {
            var result = await _processingTypeService.GetProcessingTypeList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("ProcessingType/DeleteProcessingTypeById/{processingTypeId}")]
        public async Task<Acknowledgement> DeleteProcessingTypeById([FromRoute] int processingTypeId)
        {
            return await _processingTypeService.DeleteProcessingTypeById(processingTypeId);
        }

        [HttpPost]
        [Route("ProcessingType/Create")]
        public async Task<Acknowledgement> Create([FromBody] ProcessingTypeViewModel postData)
        {
            return await _processingTypeService.CreateOrUpdateProcessingType(postData);
        }

        [HttpPut]
        [Route("ProcessingType/UpdateProcessingType/{processingTypeId}")]
        public async Task<Acknowledgement> CreateOrUpdateProcessingType([FromRoute] int processingTypeId, [FromBody] ProcessingTypeViewModel postData)
        {
            postData.Id = processingTypeId;
            return await _processingTypeService.CreateOrUpdateProcessingType(postData);
        }

        /// <summary>
        /// Get a specific processing type by ID
        /// </summary>
        /// <param name="processingTypeId">Processing Type ID</param>
        /// <returns>Processing type details</returns>
        /// <response code="200">Returns the processing type details</response>
        /// <response code="404">If the processing type is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<ProcessingTypeViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("ProcessingType/GetProcessingTypeById/{processingTypeId}")]
        public async Task<Acknowledgement<ProcessingTypeViewModel>> GetProcessingTypeById(int processingTypeId)
        {
            var ack = await _processingTypeService.GetProcessingTypeById(processingTypeId);
            return ack;
        }
    }
}

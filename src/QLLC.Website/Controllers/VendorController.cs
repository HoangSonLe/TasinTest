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
    /// Controller for managing vendors
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class VendorController : BaseController<VendorController>
    {
        private IVendorService _vendorService;
        public VendorController(
            IVendorService vendorService,
            IUserService userService,
            ILogger<VendorController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        [Route("Vendor/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of vendors with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of vendors</returns>
        /// <response code="200">Returns the list of vendors</response>
        [HttpGet]
        [Route("Vendor/GetVendorList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<VendorViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public async Task<IActionResult> GetVendorList([FromQuery] VendorSearchModel searchModel)
        {
            var result = await _vendorService.GetVendorList(searchModel);
            return Json(result);
        }

        [HttpDelete]
        [Route("Vendor/DeleteVendorById")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_VENDOR])]
        public async Task<Acknowledgement> DeleteVendorById(int vendorId)
        {
            return await _vendorService.DeleteVendorById(vendorId);
        }

        [HttpPost]
        [Route("Vendor/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_VENDOR])]
        public async Task<Acknowledgement> Create([FromBody] VendorViewModel postData)
        {
            return await _vendorService.CreateOrUpdateVendor(postData);
        }

        [HttpPut]
        [Route("Vendor/UpdateVendor/{vendorId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_VENDOR])]
        public async Task<Acknowledgement> CreateOrUpdateVendor([FromRoute] int vendorId, [FromBody] VendorViewModel postData)
        {
            postData.Id = vendorId;
            return await _vendorService.CreateOrUpdateVendor(postData);
        }

        /// <summary>
        /// Get a specific vendor by ID
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>Vendor details</returns>
        /// <response code="200">Returns the vendor details</response>
        /// <response code="404">If the vendor is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<VendorViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("Vendor/GetVendorById/{vendorId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR, (int) EActionRole.UPDATE_VENDOR])]
        public async Task<Acknowledgement<VendorViewModel>> GetVendorById(int vendorId)
        {
            var ack = await _vendorService.GetVendorById(vendorId);
            return ack;
        }
    }
}

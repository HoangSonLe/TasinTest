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
    /// Controller for managing purchase agreements
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class PurchaseAgreementController : BaseController<PurchaseAgreementController>
    {
        private IPurchaseAgreementService _purchaseAgreementService;
        public PurchaseAgreementController(
            IPurchaseAgreementService purchaseAgreementService,
            IUserService userService,
            ILogger<PurchaseAgreementController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _purchaseAgreementService = purchaseAgreementService;
        }

        [HttpGet]
        [Route("PurchaseAgreement/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("PurchaseAgreement/PAGroupIndex")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_PURCHASE_AGREEMENT])]
        public IActionResult PAGroupIndex()
        {
            return View();
        }

        /// <summary>
        /// Get a list of purchase agreements with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of purchase agreements</returns>
        /// <response code="200">Returns the list of purchase agreements</response>
        [HttpGet]
        [Route("PurchaseAgreement/GetPurchaseAgreementList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<PurchaseAgreementViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetPurchaseAgreementList([FromQuery] PurchaseAgreementSearchModel searchModel)
        {
            var result = await _purchaseAgreementService.GetPurchaseAgreementList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get a purchase agreement by ID
        /// </summary>
        /// <param name="purchaseAgreementId">Purchase agreement ID</param>
        /// <returns>Purchase agreement details</returns>
        /// <response code="200">Returns the purchase agreement details</response>
        [HttpGet]
        [Route("PurchaseAgreement/GetPurchaseAgreementById")]
        [ProducesResponseType(typeof(Acknowledgement<PurchaseAgreementViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetPurchaseAgreementById(int purchaseAgreementId)
        {
            var result = await _purchaseAgreementService.GetPurchaseAgreementById(purchaseAgreementId);
            return Json(result);
        }

        /// <summary>
        /// Update an existing purchase agreement
        /// </summary>
        /// <param name="postData">Purchase agreement data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [Route("PurchaseAgreement/UpdatePurchaseAgreement")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> UpdatePurchaseAgreement([FromBody] PurchaseAgreementViewModel postData)
        {
            if (postData.Id <= 0)
            {
                return Json(new Acknowledgement { IsSuccess = false, ErrorMessageList = new List<string> { "ID không hợp lệ" } });
            }

            var result = await _purchaseAgreementService.UpdatePurchaseAgreement(postData);
            return Json(result);
        }

        /// <summary>
        /// Delete a purchase agreement by ID
        /// </summary>
        /// <param name="purchaseAgreementId">Purchase agreement ID</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpDelete]
        [Route("PurchaseAgreement/DeletePurchaseAgreementById/{purchaseAgreementId}")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_PURCHASE_AGREEMENT])]
        public async Task<Acknowledgement> DeletePurchaseAgreementById([FromRoute] int purchaseAgreementId)
        {
            return await _purchaseAgreementService.DeletePurchaseAgreementById(purchaseAgreementId);
        }

        // PA Group (Parent PA) endpoints

        /// <summary>
        /// Get a list of PA groups with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of PA groups</returns>
        /// <response code="200">Returns the list of PA groups</response>
        [HttpGet]
        [Route("PurchaseAgreement/GetPAGroupList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<PAGroupViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetPAGroupList([FromQuery] PAGroupSearchModel searchModel)
        {
            var result = await _purchaseAgreementService.GetPAGroupList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get a PA group by GroupCode
        /// </summary>
        /// <param name="groupCode">Group code</param>
        /// <returns>PA group details</returns>
        /// <response code="200">Returns the PA group details</response>
        [HttpGet]
        [Route("PurchaseAgreement/GetPAByGroupCode")]
        [ProducesResponseType(typeof(Acknowledgement<PAGroupViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetPAByGroupCode(string groupCode)
        {
            var result = await _purchaseAgreementService.GetPAByGroupCode(groupCode);
            return Json(result);
        }

        /// <summary>
        /// Get preview data for PA group that will be created from confirmed purchase orders
        /// </summary>
        /// <returns>Preview data of the PA group to be created</returns>
        /// <response code="200">Returns the preview data</response>
        [HttpGet]
        [Route("PurchaseAgreement/GetPAGroupPreview")]
        [ProducesResponseType(typeof(Acknowledgement<PAGroupViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetPAGroupPreview()
        {
            var result = await _purchaseAgreementService.GetPAGroupPreview();
            return Json(result);
        }

        /// <summary>
        /// Create PA group from confirmed purchase orders
        /// </summary>
        /// <returns>Result of the operation with created PA group</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [Route("PurchaseAgreement/CreatePAGroup")]
        [ProducesResponseType(typeof(Acknowledgement<PAGroupViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> CreatePAGroup()
        {
            var result = await _purchaseAgreementService.CreatePAGroup();
            return Json(result);
        }

        /// <summary>
        /// Update PA group status to Completed
        /// </summary>
        /// <param name="groupCode">Group code of the PA group to complete</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [Route("PurchaseAgreement/CompletePAGroup")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> CompletePAGroup([FromQuery] string groupCode)
        {
            var result = await _purchaseAgreementService.CompletePAGroup(groupCode);
            return Json(result);
        }

        /// <summary>
        /// Update PA group status to Cancel
        /// </summary>
        /// <param name="groupCode">Group code of the PA group to cancel</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [Route("PurchaseAgreement/CancelPAGroup")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> CancelPAGroup([FromQuery] string groupCode)
        {
            var result = await _purchaseAgreementService.CancelPAGroup(groupCode);
            return Json(result);
        }
    }
}

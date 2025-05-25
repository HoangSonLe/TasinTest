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
    /// Controller for managing purchase orders
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class PurchaseOrderController : BaseController<PurchaseOrderController>
    {
        private IPurchaseOrderService _purchaseOrderService;
        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            IUserService userService,
            ILogger<PurchaseOrderController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        [Route("PurchaseOrder/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get a list of purchase orders with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of purchase orders</returns>
        /// <response code="200">Returns the list of purchase orders</response>
        [HttpGet]
        [Route("PurchaseOrder/GetPurchaseOrderList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<PurchaseOrderViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> GetPurchaseOrderList([FromQuery] PurchaseOrderSearchModel searchModel)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderList(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get a purchase order by ID
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Purchase order details</returns>
        /// <response code="200">Returns the purchase order details</response>
        [HttpGet]
        [Route("PurchaseOrder/GetPurchaseOrderById")]
        [ProducesResponseType(typeof(Acknowledgement<PurchaseOrderViewModel>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> GetPurchaseOrderById(int purchaseOrderId)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderById(purchaseOrderId);
            return Json(result);
        }

        /// <summary>
        /// Create a new purchase order
        /// </summary>
        /// <param name="postData">Purchase order data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPost]
        [Route("PurchaseOrder/CreatePurchaseOrder")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_PURCHASE_ORDER])]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderViewModel postData)
        {
            postData.Id = 0; // Ensure we're creating a new record
            var result = await _purchaseOrderService.CreateOrUpdatePurchaseOrder(postData);
            return Json(result);
        }

        /// <summary>
        /// Update an existing purchase order
        /// </summary>
        /// <param name="postData">Purchase order data</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpPut]
        [Route("PurchaseOrder/UpdatePurchaseOrder")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PURCHASE_ORDER])]
        public async Task<IActionResult> UpdatePurchaseOrder([FromBody] PurchaseOrderViewModel postData)
        {
            if (postData.Id <= 0)
            {
                return Json(new Acknowledgement { IsSuccess = false, ErrorMessageList = new List<string> { "ID không hợp lệ" } });
            }

            var result = await _purchaseOrderService.CreateOrUpdatePurchaseOrder(postData);
            return Json(result);
        }

        /// <summary>
        /// Delete a purchase order by ID
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Returns the result of the operation</response>
        [HttpDelete]
        [Route("PurchaseOrder/DeletePurchaseOrderById")]
        [ProducesResponseType(typeof(Acknowledgement), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_PURCHASE_ORDER])]
        public async Task<Acknowledgement> DeletePurchaseOrderById(int purchaseOrderId)
        {
            return await _purchaseOrderService.DeletePurchaseOrderById(purchaseOrderId);
        }
    }
}

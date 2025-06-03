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
    /// Controller for product order statistics and reporting
    /// </summary>
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ProductOrderStatisticsController : BaseController<ProductOrderStatisticsController>
    {
        private readonly IProductOrderStatisticsService _productOrderStatisticsService;

        public ProductOrderStatisticsController(
            IProductOrderStatisticsService productOrderStatisticsService,
            IUserService userService,
            ILogger<ProductOrderStatisticsController> logger,
            ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _productOrderStatisticsService = productOrderStatisticsService;
        }

        /// <summary>
        /// Get product order statistics grouped by vendor for completed Purchase Agreements
        /// </summary>
        /// <param name="searchModel">Search parameters including filters for product name, vendor, and date range</param>
        /// <returns>Statistics grouped by vendor showing product quantities, values, and PA counts</returns>
        /// <response code="200">Returns the product order statistics</response>
        /// <remarks>
        /// This endpoint provides comprehensive statistics for product orders from completed Purchase Agreements (PAs).
        ///
        /// Features:
        /// - Groups data by vendor (nhà cung cấp)
        /// - Shows product-level statistics including quantities, prices, and values
        /// - Supports filtering by product name (including Vietnamese non-unicode search)
        /// - Supports filtering by vendor and date range
        /// - Only includes PAs with "Completed" status
        /// - Optional detailed PA information per product
        ///
        /// Use cases:
        /// - Vendor performance analysis
        /// - Product demand analysis
        /// - Purchase volume reporting
        /// - Financial reporting by vendor
        ///
        /// Sample request:
        /// GET /ProductOrderStatistics/GetProductOrderStatistics?ProductName=sản phẩm&amp;DateFrom=2024-01-01&amp;DateTo=2024-12-31&amp;IncludeDetails=true
        /// </remarks>
        [HttpGet]
        [Route("ProductOrderStatistics/GetProductOrderStatistics")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<ProductOrderStatisticsViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public async Task<IActionResult> GetProductOrderStatistics([FromQuery] ProductOrderStatisticsSearchModel searchModel)
        {
            var result = await _productOrderStatisticsService.GetProductOrderStatistics(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get customer product order statistics grouped by customer for confirmed Purchase Orders
        /// </summary>
        /// <param name="searchModel">Search parameters including filters for product name, customer, and date range</param>
        /// <returns>Statistics grouped by customer showing product quantities, values, and PO counts</returns>
        /// <response code="200">Returns the customer product order statistics</response>
        /// <remarks>
        /// This endpoint provides comprehensive statistics for customer product orders from confirmed Purchase Orders (POs).
        ///
        /// Features:
        /// - Groups data by customer (khách hàng)
        /// - Shows product-level statistics including quantities, prices, and values
        /// - Supports filtering by product name (including Vietnamese non-unicode search)
        /// - Supports filtering by customer and date range
        /// - Only includes POs with "Confirmed" status
        /// - Optional detailed PO information per product
        ///
        /// Use cases:
        /// - Customer behavior analysis
        /// - Product demand analysis by customer
        /// - Purchase volume reporting by customer
        /// - Customer financial reporting
        ///
        /// Sample request:
        /// GET /ProductOrderStatistics/GetCustomerProductOrderStatistics?ProductName=sản phẩm&amp;DateFrom=2024-01-01&amp;DateTo=2024-12-31&amp;IncludeDetails=true
        /// </remarks>
        [HttpGet]
        [Route("ProductOrderStatistics/GetCustomerProductOrderStatistics")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public async Task<IActionResult> GetCustomerProductOrderStatistics([FromQuery] CustomerProductOrderStatisticsSearchModel searchModel)
        {
            var result = await _productOrderStatisticsService.GetCustomerProductOrderStatistics(searchModel);
            return Json(result);
        }

        /// <summary>
        /// Get product order statistics view page
        /// </summary>
        /// <returns>Statistics view page</returns>
        [HttpGet]
        [Route("ProductOrderStatistics/Index")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_AGREEMENT])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get customer product order statistics view page
        /// </summary>
        /// <returns>Customer statistics view page</returns>
        [HttpGet]
        [Route("ProductOrderStatistics/CustomerIndex")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PURCHASE_ORDER])]
        public IActionResult CustomerIndex()
        {
            return View();
        }
    }
}

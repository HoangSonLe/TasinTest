using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    [ApiController]
    public class ProductController : BaseController<ProductController>
    {
        private readonly IProductService _productService;
        private readonly ICommonService _commonService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
           ICommonService commonService,
           IUserService userService,
           ILogger<ProductController> logger,
           ICurrentUserContext currentUserContext) : base(logger, userService, currentUserContext)
        {
            _productService = productService;
            _commonService = commonService;
        }
        [HttpGet]
        [Route("Product/Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get list of products with paging
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of products</returns>
        [HttpGet]
        [Route("Product/GetProductList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<ProductViewModel>>>), 200)]
        public async Task<IActionResult> GetProductList([FromQuery] ProductSearchModel searchModel)
        {
            try
            {
                var result = await _productService.GetProductList(searchModel);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetProductList: {ex.Message}");
                return Json(new Acknowledgement<JsonResultPaging<List<ProductViewModel>>>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet]
        [Route("Product/GetProductById/{productId}")]
        [ProducesResponseType(typeof(Acknowledgement<ProductViewModel>), 200)]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var result = await _productService.GetProductById(productId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetProductById: {ex.Message}");
                return Json(new Acknowledgement<ProductViewModel>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="model">Product data</param>
        /// <returns>Result of operation</returns>
        [HttpPost]
        [Route("Product/Create")]
        public async Task<Acknowledgement> Create([FromBody] ProductViewModel model)
        {
            try
            {
                return await _productService.CreateOrUpdateProduct(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create Product: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="model">Product data</param>
        /// <returns>Result of operation</returns>
        [HttpPut]
        [Route("Product/UpdateProduct/{productId}")]
        public async Task<Acknowledgement> UpdateProduct([FromRoute] int productId, [FromBody] ProductViewModel model)
        {
            try
            {
                model.Id = productId;
                return await _productService.CreateOrUpdateProduct(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update Product: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Delete a product by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Result of operation</returns>
        [HttpDelete]
        [Route("Product/DeleteProductById")]
        public async Task<Acknowledgement> DeleteProductById(int productId)
        {
            try
            {
                return await _productService.DeleteProductById(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteProductById: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Get product dropdown list for selection
        /// </summary>
        /// <param name="searchString">Optional search string</param>
        /// <returns>List of products for dropdown</returns>
        [HttpGet]
        [Route("Product/GetProductDropdownList")]
        [ProducesResponseType(typeof(Acknowledgement<List<KendoDropdownListModel<string>>>), 200)]
        public async Task<IActionResult> GetProductDropdownList(string? searchString)
        {
            try
            {
                var result = await _commonService.GetDataOptionsDropdown(searchString, ECategoryType.Product);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetProductDropdownList: {ex.Message}");
                return Json(new Acknowledgement<List<KendoDropdownListModel<string>>>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }
    }
}

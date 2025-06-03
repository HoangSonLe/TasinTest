using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    [Authorize]
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
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PRODUCT])]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get list of products with paging
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <param name="excludeProductId">Product ID to exclude from results (optional)</param>
        /// <returns>List of products</returns>
        [HttpGet]
        [Route("Product/GetProductList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<ProductViewModel>>>), 200)]
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PRODUCT])]
        public async Task<IActionResult> GetProductList([FromQuery] ProductSearchModel searchModel, [FromQuery] int? excludeProductId = null)
        {
            try
            {
                var result = await _productService.GetProductList(searchModel, null, excludeProductId);
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
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PRODUCT])]
        public async Task<IActionResult> GetProductById([FromRoute] int productId)
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
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_PRODUCT])]
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
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_PRODUCT])]
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
        [Route("Product/DeleteProductById/{productId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_PRODUCT])]
        public async Task<Acknowledgement> DeleteProductById([FromRoute] int productId)
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
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PRODUCT])]
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

        /// <summary>
        /// Import products from Excel file
        /// </summary>
        /// <param name="file">Excel file to import</param>
        /// <returns>Import result</returns>
        [HttpPost]
        [Route("Product/ImportExcel")]
        [ProducesResponseType(typeof(Acknowledgement<ProductExcelImportResult>), 200)]
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_PRODUCT])]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                var result = await _productService.ImportProductsFromExcel(file);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImportExcel: {ex.Message}");
                return Json(new Acknowledgement<ProductExcelImportResult>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Download Excel template for product import
        /// </summary>
        /// <returns>Excel template file</returns>
        [HttpGet]
        [Route("Product/DownloadTemplate")]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var templateBytes = await _productService.GenerateProductExcelTemplate();
                var fileName = $"Product_Import_Template_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                // Set proper content type and encoding for Excel files with UTF-8 support
                Response.Headers.Add("Content-Disposition", ExcelHelper.GetContentDispositionHeader(fileName));

                return File(templateBytes, ExcelHelper.GetExcelContentType(), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadTemplate: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

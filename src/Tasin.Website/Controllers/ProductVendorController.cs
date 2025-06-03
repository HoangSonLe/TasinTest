using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing product-vendor relationships
    /// </summary>
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    public class ProductVendorController : ControllerBase
    {
        private readonly IProductVendorService _productVendorService;
        private readonly ILogger<ProductVendorController> _logger;

        public ProductVendorController(
            IProductVendorService productVendorService,
            ILogger<ProductVendorController> logger)
        {
            _productVendorService = productVendorService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of product-vendor relationships with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of product-vendor relationships</returns>
        /// <response code="200">Returns the list of product-vendor relationships</response>
        [HttpGet]
        [Route("ProductVendor/GetProductVendorList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<ProductVendorViewModel>>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public async Task<Acknowledgement<JsonResultPaging<List<ProductVendorViewModel>>>> GetProductVendorList([FromQuery] ProductVendorSearchModel searchModel)
        {
            return await _productVendorService.GetProductVendorList(searchModel);
        }

        /// <summary>
        /// Get products for a specific vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>List of products for the vendor</returns>
        /// <response code="200">Returns the list of products for the vendor</response>
        [HttpGet]
        [Route("ProductVendor/GetProductsByVendorId/{vendorId}")]
        [ProducesResponseType(typeof(Acknowledgement<List<ProductVendorViewModel>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public async Task<Acknowledgement<List<ProductVendorViewModel>>> GetProductsByVendorId([FromRoute] int vendorId)
        {
            return await _productVendorService.GetProductsByVendorId(vendorId);
        }

        /// <summary>
        /// Get vendors for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of vendors for the product</returns>
        /// <response code="200">Returns the list of vendors for the product</response>
        [HttpGet]
        [Route("ProductVendor/GetVendorsByProductId/{productId}")]
        [ProducesResponseType(typeof(Acknowledgement<List<ProductVendorViewModel>>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public async Task<Acknowledgement<List<ProductVendorViewModel>>> GetVendorsByProductId([FromRoute] int productId)
        {
            return await _productVendorService.GetVendorsByProductId(productId);
        }

        /// <summary>
        /// Get a specific product-vendor relationship
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Product-vendor relationship details</returns>
        /// <response code="200">Returns the product-vendor relationship details</response>
        /// <response code="404">If the relationship is not found</response>
        [HttpGet]
        [Route("ProductVendor/GetProductVendorById/{vendorId}/{productId}")]
        [ProducesResponseType(typeof(Acknowledgement<ProductVendorViewModel>), 200)]
        [ProducesResponseType(404)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_VENDOR])]
        public async Task<Acknowledgement<ProductVendorViewModel>> GetProductVendorById([FromRoute] int vendorId, [FromRoute] int productId)
        {
            return await _productVendorService.GetProductVendorById(vendorId, productId);
        }

        /// <summary>
        /// Create or update a product-vendor relationship
        /// </summary>
        /// <param name="model">Product-vendor data</param>
        /// <returns>Result of operation</returns>
        [HttpPost]
        [Route("ProductVendor/CreateOrUpdate")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_VENDOR, (int)EActionRole.UPDATE_VENDOR])]
        public async Task<Acknowledgement> CreateOrUpdateProductVendor([FromBody] ProductVendorViewModel model)
        {
            try
            {
                return await _productVendorService.CreateOrUpdateProductVendor(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateOrUpdateProductVendor: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Bulk add products to a vendor
        /// </summary>
        /// <param name="model">Bulk product-vendor data</param>
        /// <returns>Result of operation</returns>
        [HttpPost]
        [Route("ProductVendor/BulkAddProductsToVendor")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_VENDOR, (int)EActionRole.UPDATE_VENDOR])]
        public async Task<Acknowledgement> BulkAddProductsToVendor([FromBody] BulkProductVendorViewModel model)
        {
            try
            {
                return await _productVendorService.BulkAddProductsToVendor(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BulkAddProductsToVendor: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Delete a product-vendor relationship
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Result of operation</returns>
        [HttpDelete]
        [Route("ProductVendor/DeleteProductVendor/{vendorId}/{productId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_VENDOR])]
        public async Task<Acknowledgement> DeleteProductVendor([FromRoute] int vendorId, [FromRoute] int productId)
        {
            try
            {
                return await _productVendorService.DeleteProductVendor(vendorId, productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteProductVendor: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Delete multiple product-vendor relationships for a vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productIds">List of product IDs to remove</param>
        /// <returns>Result of operation</returns>
        [HttpDelete]
        [Route("ProductVendor/BulkDeleteProductsFromVendor/{vendorId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_VENDOR])]
        public async Task<Acknowledgement> BulkDeleteProductsFromVendor([FromRoute] int vendorId, [FromBody] List<int> productIds)
        {
            try
            {
                return await _productVendorService.BulkDeleteProductsFromVendor(vendorId, productIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BulkDeleteProductsFromVendor: {ex.Message}");
                return new Acknowledgement
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                };
            }
        }
    }
}
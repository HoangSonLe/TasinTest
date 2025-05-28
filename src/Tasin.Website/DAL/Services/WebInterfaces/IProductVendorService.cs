using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IProductVendorService : IBaseService, IDisposable
    {
        /// <summary>
        /// Get list of product-vendor relationships with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of product-vendor relationships</returns>
        Task<Acknowledgement<JsonResultPaging<List<ProductVendorViewModel>>>> GetProductVendorList(ProductVendorSearchModel searchModel);

        /// <summary>
        /// Get products for a specific vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>List of products for the vendor</returns>
        Task<Acknowledgement<List<ProductVendorViewModel>>> GetProductsByVendorId(int vendorId);

        /// <summary>
        /// Get vendors for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of vendors for the product</returns>
        Task<Acknowledgement<List<ProductVendorViewModel>>> GetVendorsByProductId(int productId);

        /// <summary>
        /// Get a specific product-vendor relationship
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Product-vendor relationship details</returns>
        Task<Acknowledgement<ProductVendorViewModel>> GetProductVendorById(int vendorId, int productId);

        /// <summary>
        /// Create or update a product-vendor relationship
        /// </summary>
        /// <param name="model">Product-vendor data</param>
        /// <returns>Result of operation</returns>
        Task<Acknowledgement> CreateOrUpdateProductVendor(ProductVendorViewModel model);

        /// <summary>
        /// Bulk add products to a vendor
        /// </summary>
        /// <param name="model">Bulk product-vendor data</param>
        /// <returns>Result of operation</returns>
        Task<Acknowledgement> BulkAddProductsToVendor(BulkProductVendorViewModel model);

        /// <summary>
        /// Delete a product-vendor relationship
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Result of operation</returns>
        Task<Acknowledgement> DeleteProductVendor(int vendorId, int productId);

        /// <summary>
        /// Delete multiple product-vendor relationships for a vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="productIds">List of product IDs to remove</param>
        /// <returns>Result of operation</returns>
        Task<Acknowledgement> BulkDeleteProductsFromVendor(int vendorId, List<int> productIds);
    }
}

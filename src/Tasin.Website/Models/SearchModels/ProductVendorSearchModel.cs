using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for product-vendor relationships
    /// </summary>
    public class ProductVendorSearchModel : SearchPagingModel<ProductVendorViewModel>
    {
        /// <summary>
        /// Vendor ID to filter by
        /// </summary>
        public int? VendorId { get; set; }

        /// <summary>
        /// Product ID to filter by
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Search string for product or vendor name
        /// </summary>
        public string? SearchString { get; set; }

        /// <summary>
        /// Minimum price filter
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Priority filter
        /// </summary>
        public int? Priority { get; set; }
    }
}

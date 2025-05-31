using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for product-vendor relationship
    /// </summary>
    public class ProductVendorViewModel : BaseViewModel
    {
        /// <summary>
        /// Vendor ID
        /// </summary>
        [Required]
        [Display(Name = "Vendor ID")]
        public int Vendor_ID { get; set; }

        /// <summary>
        /// Product ID
        /// </summary>
        [Required]
        [Display(Name = "Product ID")]
        public int Product_ID { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Unit Price
        /// </summary>
        [Display(Name = "Unit Price")]
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Priority
        /// </summary>
        [Display(Name = "Priority")]
        public int? Priority { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Navigation properties for display
        /// <summary>
        /// Vendor name for display
        /// </summary>
        [Display(Name = "Vendor Name")]
        public string? VendorName { get; set; }

        /// <summary>
        /// Product name for display
        /// </summary>
        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        /// <summary>
        /// Product code for display
        /// </summary>
        [Display(Name = "Product Code")]
        public string? ProductCode { get; set; }
    }

    /// <summary>
    /// View model for bulk adding products to vendor
    /// </summary>
    public class BulkProductVendorViewModel
    {
        /// <summary>
        /// Vendor ID
        /// </summary>
        [Required]
        public int VendorId { get; set; }

        /// <summary>
        /// List of products to add
        /// </summary>
        [Required]
        public List<ProductVendorItemViewModel> Products { get; set; } = new();
    }

    /// <summary>
    /// Individual product item for bulk operations
    /// </summary>
    public class ProductVendorItemViewModel
    {
        /// <summary>
        /// Product ID
        /// </summary>
        [Required]
        public int Product_ID { get; set; }

        /// <summary>
        /// Unit Price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Priority
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        // Display properties
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
    }
}

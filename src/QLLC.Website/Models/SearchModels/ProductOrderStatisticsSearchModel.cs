using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for product order statistics
    /// </summary>
    public class ProductOrderStatisticsSearchModel : SearchPagingModel<ProductOrderStatisticsViewModel>
    {
        /// <summary>
        /// Product name filter (supports Vietnamese non-unicode search)
        /// </summary>
        [Display(Name = "ProductName")]
        public string? ProductName { get; set; }

        /// <summary>
        /// Vendor ID filter
        /// </summary>
        [Display(Name = "Vendor_ID")]
        public int? Vendor_ID { get; set; }

        /// <summary>
        /// Date from filter (PA creation date)
        /// </summary>
        [Display(Name = "DateFrom")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date to filter (PA creation date)
        /// </summary>
        [Display(Name = "DateTo")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Include PA details in response
        /// </summary>
        [Display(Name = "IncludeDetails")]
        public bool IncludeDetails { get; set; } = false;
    }

    /// <summary>
    /// Search model for customer product order statistics
    /// </summary>
    public class CustomerProductOrderStatisticsSearchModel : SearchPagingModel<CustomerProductOrderStatisticsViewModel>
    {
        /// <summary>
        /// Product name filter (supports Vietnamese non-unicode search)
        /// </summary>
        [Display(Name = "ProductName")]
        public string? ProductName { get; set; }

        /// <summary>
        /// Customer ID filter
        /// </summary>
        [Display(Name = "Customer_ID")]
        public int? Customer_ID { get; set; }

        /// <summary>
        /// Date from filter (PO creation date)
        /// </summary>
        [Display(Name = "DateFrom")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date to filter (PO creation date)
        /// </summary>
        [Display(Name = "DateTo")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Include PO details in response
        /// </summary>
        [Display(Name = "IncludeDetails")]
        public bool IncludeDetails { get; set; } = false;
    }
}

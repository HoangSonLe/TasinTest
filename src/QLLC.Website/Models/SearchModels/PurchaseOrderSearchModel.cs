using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for purchase orders
    /// </summary>
    public class PurchaseOrderSearchModel : SearchPagingModel<PurchaseOrderViewModel>
    {
        /// <summary>
        /// Customer ID to filter by
        /// </summary>
        [Display(Name = "Customer_ID")]
        public int? Customer_ID { get; set; }

        /// <summary>
        /// Status to filter by
        /// </summary>
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}

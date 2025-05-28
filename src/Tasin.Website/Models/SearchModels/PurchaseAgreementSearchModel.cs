using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for purchase agreement
    /// </summary>
    public class PurchaseAgreementSearchModel : SearchPagingModel<PurchaseAgreementViewModel>
    {
        /// <summary>
        /// Vendor ID filter
        /// </summary>
        public int? Vendor_ID { get; set; }

        /// <summary>
        /// Status filter
        /// </summary>
        public EPAStatus? Status { get; set; }

        /// <summary>
        /// Group Code filter
        /// </summary>
        public string? GroupCode { get; set; }

        /// <summary>
        /// Date from filter
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date to filter
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}

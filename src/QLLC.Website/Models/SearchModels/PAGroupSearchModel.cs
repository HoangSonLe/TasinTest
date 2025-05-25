using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for PA (Parent Purchase Agreement)
    /// </summary>
    public class PAGroupSearchModel : SearchPagingModel<PAGroupViewModel>
    {
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

        /// <summary>
        /// Vendor ID filter (for filtering by specific vendor in child PAs)
        /// </summary>
        public int? Vendor_ID { get; set; }
    }
}

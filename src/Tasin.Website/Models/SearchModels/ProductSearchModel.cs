using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for products
    /// </summary>
    public class ProductSearchModel : SearchPagingModel<ProductViewModel>
    {
        /// <summary>
        /// Unit ID to filter by
        /// </summary>
        [Display(Name = "Unit_ID")]
        public int? Unit_ID { get; set; }

        /// <summary>
        /// Category ID to filter by
        /// </summary>
        [Display(Name = "Category_ID")]
        public int? Category_ID { get; set; }

        /// <summary>
        /// Processing Type ID to filter by
        /// </summary>
        [Display(Name = "ProcessingType_ID")]
        public int? ProcessingType_ID { get; set; }

        /// <summary>
        /// Material ID to filter by
        /// </summary>
        [Display(Name = "Material_ID")]
        public int? Material_ID { get; set; }


        /// <summary>
        /// Include discontinued products
        /// </summary>
        [Display(Name = "IncludeDiscontinued")]
        public bool IncludeDiscontinued { get; set; } = false;
    }
}

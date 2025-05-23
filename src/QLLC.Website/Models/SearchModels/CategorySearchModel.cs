using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for categories
    /// </summary>
    public class CategorySearchModel : SearchPagingModel<CategoryViewModel>
    {
        /// <summary>
        /// Parent category ID to filter by
        /// </summary>
        [Display(Name = "Parent_ID")]
        public int? Parent_ID { get; set; }
    }
}

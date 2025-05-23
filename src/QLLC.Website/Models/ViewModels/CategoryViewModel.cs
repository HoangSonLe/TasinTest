using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for category information
    /// </summary>
    public class CategoryViewModel : BaseViewModel
    {
        /// <summary>
        /// Category ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Non-unicode name for searching
        /// </summary>
        [Display(Name = "NameNonUnicode")]
        public string? NameNonUnicode { get; set; }

        /// <summary>
        /// English name
        /// </summary>
        [Display(Name = "Name_EN")]
        public string? Name_EN { get; set; }

        /// <summary>
        /// Parent category ID
        /// </summary>
        [Display(Name = "Parent_ID")]
        public int? Parent_ID { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Parent category name (for display purposes)
        /// </summary>
        [Display(Name = "ParentName")]
        public string? ParentName { get; set; }
    }
}

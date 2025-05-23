using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for material information
    /// </summary>
    public class MaterialViewModel : BaseViewModel
    {
        /// <summary>
        /// Material ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// Name of the material
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
        /// Parent material ID
        /// </summary>
        [Display(Name = "Parent_ID")]
        public int? Parent_ID { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Parent material name (for display purposes)
        /// </summary>
        [Display(Name = "ParentName")]
        public string? ParentName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for special product tax rate information
    /// </summary>
    public class SpecialProductTaxRateViewModel : BaseViewModel
    {
        /// <summary>
        /// Special Product Tax Rate ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Display(Name = "Code")]
        public string? Code { get; set; }

        /// <summary>
        /// Name of the special product tax rate
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
        /// Description
        /// </summary>
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Status of the special product tax rate
        /// </summary>
        [Display(Name = "Status")]
        public ECommonStatus Status { get; set; } = ECommonStatus.Actived;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);
    }
}

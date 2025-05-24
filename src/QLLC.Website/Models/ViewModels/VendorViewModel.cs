using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for vendor information
    /// </summary>
    public class VendorViewModel : BaseViewModel
    {
        /// <summary>
        /// Vendor ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// Full name of the vendor
        /// </summary>
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        /// <summary>
        /// Non-unicode name for searching
        /// </summary>
        [Required]
        [Display(Name = "NameNonUnicode")]
        public required string NameNonUnicode { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        [Display(Name = "Address")]
        public string? Address { get; set; }


        /// <summary>
        /// Name of the user who last updated this record
        /// </summary>
        [Display(Name = "Updated By")]
        public string UpdatedByName { get; set; }

        /// <summary>
        /// Status of the vendor
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

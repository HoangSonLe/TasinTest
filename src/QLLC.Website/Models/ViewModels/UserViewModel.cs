using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for user information
    /// </summary>
    public class UserViewModel : BaseViewModel
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username for login
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// User password (hashed)
        /// </summary>
        [Display(Name = "Password")]
        public string? Password { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        [Required]
        [Display(Name = "Full Name")]
        public string? Name { get; set; }

        /// <summary>
        /// Non-unicode name for searching
        /// </summary>
        [Display(Name = "NameNonUnicode")]
        public string? NameNonUnicode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        //[EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        [Display(Name = "Address")]
        public string? Address { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [Phone]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        /// <summary>
        /// Không dùng nữa
        /// </summary>
        //public int TypeAccount { get; set; }

        /// <summary>
        /// List of role IDs assigned to the user
        /// </summary>
        [Display(Name = "Roles")]
        public List<int> RoleIdList { get; set; } = new List<int>();

        /// <summary>
        /// List of role view models
        /// </summary>
        [Ignore]
        public List<RoleViewModel> RoleViewList { get; set; } = new List<RoleViewModel>();

        /// <summary>
        /// List of action permissions
        /// </summary>
        [Ignore]
        public List<int> EnumActionList { get; set; } = new List<int>();

        /// <summary>
        /// Comma-separated list of role names
        /// </summary>
        [Ignore]
        [Display(Name = "Role Names")]
        public string? RoleName { get; set; }

        /// <summary>
        /// Name of the user who last updated this record
        /// </summary>
        [Display(Name = "Updated By")]
        public string? UpdatedByName { get; set; }

        /// <summary>
        /// Status of the user
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

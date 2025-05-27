using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for users
    /// </summary>
    public class UserSearchModel : SearchPagingModel<UserViewModel>
    {
        /// <summary>
        /// Search string to filter users by name, username, or phone
        /// </summary>
        [Display(Name = "Search")]
        public string? SearchString { get; set; }

        /// <summary>
        /// Comma-separated list of role IDs to filter users by role
        /// </summary>
        [Display(Name = "Role IDs")]
        public string? RoleIdListString { get; set; }

        /// <summary>
        /// List of role IDs parsed from RoleIdListString
        /// </summary>
        public List<int>? RoleIdList => !string.IsNullOrEmpty(RoleIdListString) ? RoleIdListString.Split(",").Select(i => Int32.Parse(i)).ToList() : null;
    }
}

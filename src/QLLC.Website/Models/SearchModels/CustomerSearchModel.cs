using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for users
    /// </summary>
    public class CustomerSearchModel : SearchPagingModel<CustomerViewModel>
    {
        /// <summary>
        /// Comma-separated list of role IDs to filter users by role
        /// </summary>
        [Display(Name = "TypeCode")]
        public ECustomerType TypeCode { get; set; }

    }
}

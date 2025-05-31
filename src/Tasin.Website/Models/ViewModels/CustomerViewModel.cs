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
    public class CustomerViewModel : BaseViewModel
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Display(Name = "Code")]
        public string? Code { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        /// <summary>
        /// Non-unicode name for searching
        /// </summary>
        [Display(Name = "NameNonUnicode")]
        public string? NameNonUnicode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [EmailAddress]
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
        [Display(Name = "PhoneContact")]
        public string? PhoneContact { get; set; }

        [Display(Name = "Type")]
        public ECustomerType Type { get; set; }

        [Display(Name = "TypeName")]
        public string TypeName => EnumHelper.GetEnumDescription(Type);

        [Display(Name = "TaxCode")]
        public string? TaxCode { get; set; }

        /// <summary>
        /// Name of the user who last updated this record
        /// </summary>
        [Display(Name = "Updated By")]
        public new string? UpdatedByName { get; set; }

        /// <summary>
        /// Status of the customer
        /// </summary>
        [Display(Name = "Status")]
        public ECommonStatus Status { get; set; } = ECommonStatus.Actived;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);
    }

    /// <summary>
    /// Model for Excel import of customers
    /// </summary>
    public class CustomerExcelImportModel
    {
        public int RowNumber { get; set; }
        public string Name { get; set; } = "";
        public string TypeText { get; set; } = "";
        public string? PhoneContact { get; set; }
        public string? Email { get; set; }
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Result of customer Excel import operation
    /// </summary>
    public class CustomerExcelImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int FailedRows { get; set; }
        public List<CustomerExcelImportError> Errors { get; set; } = new List<CustomerExcelImportError>();
    }

    /// <summary>
    /// Error information for failed customer import rows
    /// </summary>
    public class CustomerExcelImportError
    {
        public int RowNumber { get; set; }
        public string CustomerName { get; set; } = "";
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}

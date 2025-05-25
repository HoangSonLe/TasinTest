using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for purchase agreement information (Parent PA that aggregates child PAs)
    /// </summary>
    public class PAGroupViewModel : BaseViewModel
    {
        /// <summary>
        /// Group Code - unique identifier for this PA group
        /// </summary>
        [Required]
        [Display(Name = "GroupCode")]
        public string GroupCode { get; set; }

        /// <summary>
        /// Total Price of all child PAs
        /// </summary>
        [Display(Name = "TotalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Note for the PA group
        /// </summary>
        [Display(Name = "Note")]
        public string? Note { get; set; }

        /// <summary>
        /// Status of the PA group
        /// </summary>
        [Required]
        [Display(Name = "Status")]
        public EPAStatus Status { get; set; } = EPAStatus.New;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);

        /// <summary>
        /// List of child PAs grouped by vendor
        /// </summary>
        [Display(Name = "ChildPAs")]
        public List<PurchaseAgreementViewModel> ChildPAs { get; set; } = new List<PurchaseAgreementViewModel>();

        /// <summary>
        /// Total number of vendors involved
        /// </summary>
        [Display(Name = "VendorCount")]
        public int VendorCount => ChildPAs.Count;

        /// <summary>
        /// Total number of items across all child PAs
        /// </summary>
        [Display(Name = "TotalItemCount")]
        public int TotalItemCount => ChildPAs.Sum(pa => pa.PurchaseAgreementItems.Count);
    }

    /// <summary>
    /// View model for individual purchase agreement (Child PA for specific vendor)
    /// </summary>
    public class PurchaseAgreementViewModel : BaseViewModel
    {
        /// <summary>
        /// Purchase Agreement ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Purchase Agreement Code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        [Required]
        [Display(Name = "Vendor_ID")]
        public int Vendor_ID { get; set; }

        /// <summary>
        /// Vendor Name
        /// </summary>
        [Display(Name = "VendorName")]
        public string VendorName { get; set; }

        /// <summary>
        /// Group Code - links to parent PA
        /// </summary>
        [Required]
        [Display(Name = "GroupCode")]
        public string GroupCode { get; set; }

        /// <summary>
        /// Total Price for this vendor
        /// </summary>
        [Display(Name = "TotalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Note
        /// </summary>
        [Display(Name = "Note")]
        public string? Note { get; set; }

        /// <summary>
        /// Status of the purchase agreement
        /// </summary>
        [Required]
        [Display(Name = "Status")]
        public EPAStatus Status { get; set; } = EPAStatus.New;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);

        /// <summary>
        /// Purchase Agreement Items for this vendor
        /// </summary>
        [Display(Name = "PurchaseAgreementItems")]
        public List<PurchaseAgreementItemViewModel> PurchaseAgreementItems { get; set; } = new List<PurchaseAgreementItemViewModel>();
    }
}

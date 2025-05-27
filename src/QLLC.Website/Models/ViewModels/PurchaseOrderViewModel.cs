using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for purchase order information
    /// </summary>
    public class PurchaseOrderViewModel : BaseViewModel
    {
        /// <summary>
        /// Purchase Order ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Purchase Order Code
        /// </summary>
        [Display(Name = "Code")]
        public string? Code { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        [Required]
        [Display(Name = "Customer_ID")]
        public int Customer_ID { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        [Display(Name = "CustomerName")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Total Price
        /// </summary>
        [Display(Name = "TotalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Total Price No Tax
        /// </summary>
        [Display(Name = "TotalPriceNoTax")]
        public decimal TotalPriceNoTax { get; set; }

        /// <summary>
        /// Status of the purchase order
        /// </summary>
        [Required]
        [Display(Name = "Status")]
        public EPOStatus Status { get; set; } = EPOStatus.New;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);

        /// <summary>
        /// Purchase Order Items
        /// </summary>
        [Display(Name = "PurchaseOrderItems")]
        public List<PurchaseOrderItemViewModel> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItemViewModel>();
    }
}

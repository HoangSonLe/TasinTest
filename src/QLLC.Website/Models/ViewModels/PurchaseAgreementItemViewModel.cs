using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for purchase agreement item information
    /// </summary>
    public class PurchaseAgreementItemViewModel
    {
        /// <summary>
        /// Purchase Agreement ID
        /// </summary>
        [Required]
        [Display(Name = "PA_ID")]
        public int PA_ID { get; set; }

        /// <summary>
        /// Product ID
        /// </summary>
        [Required]
        [Display(Name = "Product_ID")]
        public int Product_ID { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [Display(Name = "ProductName")]
        public string ProductName { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Required]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit ID
        /// </summary>
        [Display(Name = "Unit_ID")]
        public int? Unit_ID { get; set; }

        /// <summary>
        /// Unit Name
        /// </summary>
        [Display(Name = "UnitName")]
        public string UnitName { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Purchase Order Item ID List (comma separated)
        /// </summary>
        [Display(Name = "PO_Item_ID_List")]
        public string? PO_Item_ID_List { get; set; }
    }
}

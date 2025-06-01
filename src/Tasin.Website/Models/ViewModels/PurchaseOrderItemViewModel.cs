using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for purchase order item information
    /// </summary>
    public class PurchaseOrderItemViewModel
    {
        /// <summary>
        /// Purchase Order ID
        /// </summary>
        [Required]
        [Display(Name = "PO_ID")]
        public int PO_ID { get; set; }

        /// <summary>
        /// Item ID
        /// </summary>
        [Required]
        [Display(Name = "ID")]
        public int ID { get; set; }

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
        public string? ProductName { get; set; }

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
        public string? UnitName { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Tax Rate
        /// </summary>
        [Display(Name = "TaxRate")]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Loss Rate
        /// </summary>
        [Display(Name = "LossRate")]
        public decimal? LossRate { get; set; }

        /// <summary>
        /// Additional Cost
        /// </summary>
        [Display(Name = "AdditionalCost")]
        public decimal? AdditionalCost { get; set; }

        /// <summary>
        /// Processing Fee
        /// </summary>
        [Display(Name = "ProcessingFee")]
        public decimal? ProcessingFee { get; set; }

        /// <summary>
        /// Note
        /// </summary>
        [Display(Name = "Note")]
        public string? Note { get; set; }
    }
}

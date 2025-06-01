using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for product information
    /// </summary>
    public class ProductViewModel : BaseViewModel
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Display(Name = "Code")]
        public string? Code { get; set; }

        /// <summary>
        /// Name of the product
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
        /// Unit ID
        /// </summary>
        [Display(Name = "Unit_ID")]
        public int? Unit_ID { get; set; }

        /// <summary>
        /// Unit name
        /// </summary>
        public string? UnitName { get; set; }

        /// <summary>
        /// Category ID
        /// </summary>
        [Display(Name = "Category_ID")]
        public int? Category_ID { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Processing Type
        /// </summary>
        [Display(Name = "ProcessingType")]
        public EProcessingType ProcessingType { get; set; } = EProcessingType.Material;

        /// <summary>
        /// Processing Type name for display
        /// </summary>
        public string ProcessingTypeName => EnumHelper.GetEnumDescriptionByEnum(ProcessingType);

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
        /// Is Material
        /// </summary>
        [Display(Name = "IsMaterial")]
        public bool IsMaterial { get; set; } = false;

        /// <summary>
        /// Profit Margin
        /// </summary>
        [Display(Name = "ProfitMargin")]
        public decimal? ProfitMargin { get; set; }

        /// <summary>
        /// Default Price
        /// </summary>
        [Display(Name = "DefaultPrice")]
        public decimal? DefaultPrice { get; set; }

        /// <summary>
        /// Note
        /// </summary>
        [Display(Name = "Note")]
        public string? Note { get; set; }

        /// <summary>
        /// Is Discontinued
        /// </summary>
        [Display(Name = "IsDiscontinued")]
        public bool IsDiscontinued { get; set; } = false;

        /// <summary>
        /// Processing Fee
        /// </summary>
        [Display(Name = "ProcessingFee")]
        public decimal? ProcessingFee { get; set; }

        /// <summary>
        /// Company Tax Rate
        /// </summary>
        [Display(Name = "CompanyTaxRate")]
        public decimal? CompanyTaxRate { get; set; }

        /// <summary>
        /// Consumer Tax Rate
        /// </summary>
        [Display(Name = "ConsumerTaxRate")]
        public decimal? ConsumerTaxRate { get; set; }

        /// <summary>
        /// Special Product Tax Rate ID
        /// </summary>
        [Display(Name = "SpecialProductTaxRate_ID")]
        public int? SpecialProductTaxRate_ID { get; set; }

        /// <summary>
        /// Special Product Tax Rate name
        /// </summary>
        public string? SpecialProductTaxRateName { get; set; }

        /// <summary>
        /// Parent Product ID
        /// </summary>
        [Display(Name = "ParentID")]
        public int? ParentID { get; set; }

        /// <summary>
        /// Parent Product name
        /// </summary>
        public string? ParentName { get; set; }

        /// <summary>
        /// Status of the product
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

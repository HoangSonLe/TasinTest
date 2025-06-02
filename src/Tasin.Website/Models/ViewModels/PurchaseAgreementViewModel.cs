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
        [Display(Name = "GroupCode")]
        public string? GroupCode { get; set; }

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
        public List<PurchaseAgreementViewModel>? ChildPAs { get; set; }

        /// <summary>
        /// Total number of vendors involved
        /// </summary>
        [Display(Name = "VendorCount")]
        public int VendorCount => ChildPAs?.Count ?? 0;

        /// <summary>
        /// Total number of items across all child PAs
        /// </summary>
        [Display(Name = "TotalItemCount")]
        public int TotalItemCount => ChildPAs?.Sum(pa => pa.PurchaseAgreementItems?.Count ?? 0) ?? 0;
    }

    /// <summary>
    /// View model for editable PA group preview that allows vendor assignment changes
    /// </summary>
    public class EditablePAGroupPreviewViewModel : BaseViewModel
    {
        /// <summary>
        /// Group Code - will be generated automatically
        /// </summary>
        [Display(Name = "GroupCode")]
        public string? GroupCode { get; set; }

        /// <summary>
        /// Total Price of all items
        /// </summary>
        [Display(Name = "TotalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Status of the PA group
        /// </summary>
        [Display(Name = "Status")]
        public EPAStatus Status { get; set; } = EPAStatus.New;

        /// <summary>
        /// Status description for display purposes
        /// </summary>
        [Display(Name = "Status Name")]
        public string StatusName => EnumHelper.GetEnumDescriptionByEnum(Status);

        /// <summary>
        /// List of products with editable vendor assignments
        /// </summary>
        [Display(Name = "ProductVendorMappings")]
        public List<ProductVendorMappingViewModel>? ProductVendorMappings { get; set; }

        /// <summary>
        /// Total number of products
        /// </summary>
        [Display(Name = "TotalItemCount")]
        public int TotalItemCount => ProductVendorMappings?.Count ?? 0;
    }

    /// <summary>
    /// View model for product-vendor mapping in editable preview
    /// </summary>
    public class ProductVendorMappingViewModel
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int Product_ID { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        public string? ProductCode { get; set; }

        /// <summary>
        /// Total Quantity from all purchase orders
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// Unit ID
        /// </summary>
        public int? Unit_ID { get; set; }

        /// <summary>
        /// Unit Name
        /// </summary>
        public string? UnitName { get; set; }

        /// <summary>
        /// Price per unit
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Total amount for this product
        /// </summary>
        public decimal TotalAmount => (Price ?? 0) * TotalQuantity;

        /// <summary>
        /// Currently assigned vendor ID
        /// </summary>
        public int Vendor_ID { get; set; }

        /// <summary>
        /// Currently assigned vendor name
        /// </summary>
        public string? VendorName { get; set; }

        /// <summary>
        /// List of available vendors for this product
        /// </summary>
        public List<VendorOptionViewModel>? AvailableVendors { get; set; }

        /// <summary>
        /// Purchase Order Item IDs that contribute to this product
        /// </summary>
        public string? PO_Item_ID_List { get; set; }
    }

    /// <summary>
    /// View model for vendor options in dropdown
    /// </summary>
    public class VendorOptionViewModel
    {
        /// <summary>
        /// Vendor ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Vendor Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Vendor Code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Unit price for this vendor-product combination
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Display text for dropdown (Name + Code)
        /// </summary>
        public string Text => !string.IsNullOrEmpty(Code) ? $"{Name} ({Code})" : Name ?? "";
    }

    /// <summary>
    /// Request model for creating PA group with custom vendor mappings
    /// </summary>
    public class CreatePAGroupWithMappingRequest
    {
        /// <summary>
        /// List of product-vendor mappings
        /// </summary>
        public List<ProductVendorMappingViewModel>? ProductVendorMappings { get; set; }
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
        [Display(Name = "Code")]
        public string? Code { get; set; }

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
        public string? VendorName { get; set; }

        /// <summary>
        /// Group Code - links to parent PA
        /// </summary>
        [Display(Name = "GroupCode")]
        public string? GroupCode { get; set; }

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
        public List<PurchaseAgreementItemViewModel>? PurchaseAgreementItems { get; set; }
    }
}

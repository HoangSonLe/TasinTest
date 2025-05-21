﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Product : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public string? NameNonUnicode { get; set; }
        public string? Name_EN { get; set; }
        public int? Unit_ID { get; set; }
        public int? Category_ID { get; set; }
        public int? ProcessingType_ID { get; set; }
        public decimal? TaxRate { get; set; }
        public int? TaxRateConfig_ID { get; set; }
        public decimal? LossRate { get; set; }
        public int? Material_ID { get; set; }
        public decimal? ProfitMargin { get; set; }
        public string? Note { get; set; }
        public bool IsDiscontinued { get; set; } = false;
        public decimal? ProcessingFee { get; set; }
        public string? Status { get; set; }
        public bool IsActived { get; set; } = true;

        // Navigation properties
        [ForeignKey("Unit_ID")]
        [NotMapped]
        public virtual Unit? Unit { get; set; }
        
        [ForeignKey("Category_ID")]
        [NotMapped]
        public virtual Category? Category { get; set; }
        
        [ForeignKey("ProcessingType_ID")]
        [NotMapped]
        public virtual ProcessingType? ProcessingType { get; set; }
        
        [ForeignKey("TaxRateConfig_ID")]
        [NotMapped]
        public virtual TaxRateConfig? TaxRateConfig { get; set; }
        
        [ForeignKey("Material_ID")]
        [NotMapped]
        public virtual Material? Material { get; set; }
        
        [NotMapped]
        public virtual ICollection<Product_Vendor>? ProductVendors { get; set; }
        
        [NotMapped]
        public virtual ICollection<Purchase_Order_Item>? PurchaseOrderItems { get; set; }
        
        [NotMapped]
        public virtual ICollection<Purchase_Agreement_Item>? PurchaseAgreementItems { get; set; }
    }
}

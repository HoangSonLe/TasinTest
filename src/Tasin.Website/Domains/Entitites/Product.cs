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
        public required string NameNonUnicode { get; set; }
        public string? Name_EN { get; set; }
        public int? Unit_ID { get; set; }
        public int? Category_ID { get; set; }
        public int? ProcessingType_ID { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? LossRate { get; set; }
        public bool IsMaterial { get; set; } = false;
        public decimal? ProfitMargin { get; set; }
        public string? Note { get; set; }
        public bool IsDiscontinued { get; set; } = false;
        public decimal? ProcessingFee { get; set; }
        public decimal CompanyTaxRate { get; set; }
        public decimal ConsumerTaxRate { get; set; }
        public int? SpecialProductTaxRate_ID { get; set; }

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

        [ForeignKey("SpecialProductTaxRate_ID")]
        [NotMapped]
        public virtual SpecialProductTaxRate? SpecialProductTaxRate { get; set; }

        [NotMapped]
        public virtual ICollection<Product_Vendor>? ProductVendors { get; set; }

        [NotMapped]
        public virtual ICollection<Purchase_Order_Item>? PurchaseOrderItems { get; set; }

        [NotMapped]
        public virtual ICollection<Purchase_Agreement_Item>? PurchaseAgreementItems { get; set; }
    }
}

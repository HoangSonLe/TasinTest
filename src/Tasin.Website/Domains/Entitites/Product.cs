﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tasin.Website.Common.Enums;

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
        public EProcessingType ProcessingType { get; set; } = EProcessingType.Material;
        public decimal? LossRate { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? DefaultPrice { get; set; }
        public string? Note { get; set; }
        public bool IsDiscontinued { get; set; } = false;
        public decimal? ProcessingFee { get; set; }
        public decimal? CompanyTaxRate { get; set; }
        public decimal? ConsumerTaxRate { get; set; }
        public int? SpecialProductTaxRate_ID { get; set; }
        public int? ParentID { get; set; }

        // Navigation properties
        [ForeignKey("Unit_ID")]
        [NotMapped]
        public virtual Unit? Unit { get; set; }

        [ForeignKey("Category_ID")]
        [NotMapped]
        public virtual Category? Category { get; set; }

        [ForeignKey("SpecialProductTaxRate_ID")]
        [NotMapped]
        public virtual SpecialProductTaxRate? SpecialProductTaxRate { get; set; }

        [ForeignKey("ParentID")]
        [NotMapped]
        public virtual Product? Parent { get; set; }

        [NotMapped]
        public virtual ICollection<Product>? Children { get; set; }

        [NotMapped]
        public virtual ICollection<Product_Vendor>? ProductVendors { get; set; }

        [NotMapped]
        public virtual ICollection<Purchase_Order_Item>? PurchaseOrderItems { get; set; }

        [NotMapped]
        public virtual ICollection<Purchase_Agreement_Item>? PurchaseAgreementItems { get; set; }
    }
}

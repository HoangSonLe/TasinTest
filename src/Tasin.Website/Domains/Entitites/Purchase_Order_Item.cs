﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Purchase_Order_Item
    {
        public int PO_ID { get; set; }
        public int ID { get; set; }
        public int Product_ID { get; set; }
        public decimal Quantity { get; set; }
        public int? Unit_ID { get; set; }
        public decimal? Price { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? LossRate { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? ProcessingFee { get; set; }
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("PO_ID")]
        [NotMapped]
        public virtual Purchase_Order? PurchaseOrder { get; set; }

        [ForeignKey("Product_ID")]
        [NotMapped]
        public virtual Product? Product { get; set; }

        [ForeignKey("Unit_ID")]
        [NotMapped]
        public virtual Unit? Unit { get; set; }
    }
}

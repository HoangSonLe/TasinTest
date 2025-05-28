﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Purchase_Agreement_Item
    {
        public int PA_ID { get; set; }
        public int Product_ID { get; set; }
        public decimal Quantity { get; set; }
        public int? Unit_ID { get; set; }
        public decimal? Price { get; set; }
        public string? PO_Item_ID_List { get; set; }

        // Navigation properties
        [ForeignKey("PA_ID")]
        [NotMapped]
        public virtual Purchase_Agreement? PurchaseAgreement { get; set; }
        
        [ForeignKey("Product_ID")]
        [NotMapped]
        public virtual Product? Product { get; set; }
        
        [ForeignKey("Unit_ID")]
        [NotMapped]
        public virtual Unit? Unit { get; set; }
    }
}

﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Purchase_Order : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public int Customer_ID { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceNoTax { get; set; }
        public required string Code { get; set; }
        public string? Status { get; set; }

        // Navigation properties
        [ForeignKey("Customer_ID")]
        [NotMapped]
        public virtual Customer? Customer { get; set; }
        
        [NotMapped]
        public virtual ICollection<Purchase_Order_Item>? PurchaseOrderItems { get; set; }
    }
}

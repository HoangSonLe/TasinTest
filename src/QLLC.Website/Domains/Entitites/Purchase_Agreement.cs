﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Purchase_Agreement : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public int Vendor_ID { get; set; }
        public string? Note { get; set; }
        public required string Code { get; set; }
        public required string GroupCode { get; set; }
        public decimal TotalPrice { get; set; }
        public new string Status { get; set; }

        // Navigation properties
        [ForeignKey("Vendor_ID")]
        [NotMapped]
        public virtual Vendor? Vendor { get; set; }

        [NotMapped]
        public virtual ICollection<Purchase_Agreement_Item>? PurchaseAgreementItems { get; set; }
    }
}

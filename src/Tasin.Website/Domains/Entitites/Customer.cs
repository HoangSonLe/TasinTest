﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Customer : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public string NameNonUnicode { get; set; } = "";
        public string Type { get; set; }
        public string? PhoneContact { get; set; }
        public string? Email { get; set; }
        public string? TaxCode { get; set; }
        public string? Address { get; set; }

        // Navigation properties
        [NotMapped]
        public virtual ICollection<Purchase_Order>? PurchaseOrders { get; set; }
    }
}

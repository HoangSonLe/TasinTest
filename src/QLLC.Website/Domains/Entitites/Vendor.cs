﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Vendor : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public string? NameNonUnicode { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }
        public bool IsActived { get; set; } = true;

        // Navigation properties
        [NotMapped]
        public virtual ICollection<Product_Vendor>? ProductVendors { get; set; }
        [NotMapped]
        public virtual ICollection<Purchase_Agreement>? PurchaseAgreements { get; set; }
    }
}

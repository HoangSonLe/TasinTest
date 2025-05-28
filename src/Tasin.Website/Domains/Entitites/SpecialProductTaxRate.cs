﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class SpecialProductTaxRate : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public required string NameNonUnicode { get; set; }
        public string? Name_EN { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        [NotMapped]
        public virtual ICollection<TaxRateConfig>? TaxRateConfigs { get; set; }

        [NotMapped]
        public virtual ICollection<Product>? Products { get; set; }
    }
}

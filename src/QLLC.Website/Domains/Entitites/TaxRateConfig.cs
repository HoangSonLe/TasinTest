﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class TaxRateConfig : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public decimal CompanyTaxRate { get; set; }
        public decimal ConsumerTaxRate { get; set; }
        public int? SpecialProductTaxRate_ID { get; set; }

        // Navigation properties
        [ForeignKey("SpecialProductTaxRate_ID")]
        [NotMapped]
        public virtual SpecialProductTaxRate? SpecialProductTaxRate { get; set; }

        [NotMapped]
        public virtual ICollection<Product>? Products { get; set; }
    }
}

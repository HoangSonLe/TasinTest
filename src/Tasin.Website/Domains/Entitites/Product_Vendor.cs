﻿﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Product_Vendor
    {
        public int Vendor_ID { get; set; }
        public int Product_ID { get; set; }
        public decimal? Price { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Priority { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        [ForeignKey("Vendor_ID")]
        [NotMapped]
        public virtual Vendor? Vendor { get; set; }

        [ForeignKey("Product_ID")]
        [NotMapped]
        public virtual Product? Product { get; set; }
    }
}

﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class ProcessingType : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public required string NameNonUnicode { get; set; }
        public string? Name_EN { get; set; }
        public string? Description { get; set; }

        // Navigation properties removed - ProcessingType is now an enum in Product and Purchase_Order_Item
    }
}

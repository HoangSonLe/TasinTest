﻿﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Material : BaseAuditableEntity
    {
        [Key]
        public int ID { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public required string NameNonUnicode { get; set; }
        public string? Name_EN { get; set; }
        public int? Parent_ID { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        [ForeignKey("Parent_ID")]
        [NotMapped]
        public virtual Material? Parent { get; set; }

        [NotMapped]
        public virtual ICollection<Material>? Children { get; set; }


    }
}

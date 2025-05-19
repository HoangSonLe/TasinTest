using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class StorageMap : BaseAuditableEntity
    {
        [Key]
        public long Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string? Image { get; set; }
        public required string Location { get; set; }
        [Column(TypeName = "varchar")]
        public required string LocationNonUnicode { get; set; }
        public required string Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Tenant : BaseAuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        [Column(TypeName = "varchar")]
        public required string NameNonUnicode { get; set; }
        public required string Address { get; set; }
        [Column(TypeName = "varchar")]
        public required string AddressNonUnicode { get; set; }
        public Config Configs { get; set; }
    }
}

using Tasin.Website.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Urn : BaseAuditableEntity
    {
        [Key]
        public long Id { get; set; }
        public int TenantId { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "varchar")]
        public required string NameNonUnicode { get; set; }
        public required string DharmaName { get; set; }
        [Column(TypeName = "varchar")]
        public required string DharmaNameNonUnicode { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DeathDate { get; set; }
        public EGender Gender { get; set; }
        public EUrnType UrnType { get; set; }
        public string Note { get; set; }
        public string TowerLocation { get; set; }
        [Column(TypeName = "varchar")]
        public string TowerLocationNonUnicode { get; set; }
        public string CabinetName { get; set; }
        [Column(TypeName = "varchar")]
        public string CabinetNameNonUnicode { get; set; }
        public int RowNumber { get; set; }
        public int BoxNumber { get; set; }
        public int LocationNumber { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string? FileImageUrl { get; set; }
        public Tenant Tenant { get; set; }
        public ICollection<User_Urn> FamilyMembers { get; set; } = new List<User_Urn>();
    }
}

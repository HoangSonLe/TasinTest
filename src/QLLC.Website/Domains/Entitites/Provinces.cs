using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Provinces 
    {
        [Key]
        public int id { get; set; }
        public required string name { get; set; }
        public required string code { get; set; }
        [Column(TypeName = "varchar")]
        public required string name_en { get; set; }

    }
}

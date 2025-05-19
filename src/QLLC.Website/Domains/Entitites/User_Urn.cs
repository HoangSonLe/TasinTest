using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Domains.Entitites
{
    public class User_Urn
    {
        public long UrnId { get; set; }
        public Urn Urn { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}

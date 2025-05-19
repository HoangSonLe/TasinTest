using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Domains.Entitites
{
    public class Reminder : BaseAuditableEntity
    {
        [Key]
        public long Id { get; set; }
        public int UserId { get; set; }
        public DateTime RemindDate { get; set; }
        public string Content { get; set; }
        //public long UrnId { get; set; }
        public User User { get; set; }
        //public Urn Urn { get; set; }

    }
}

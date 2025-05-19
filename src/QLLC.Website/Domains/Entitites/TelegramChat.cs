using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Domains.Entitites
{
    public class TelegramChat : BaseAuditableEntity
    {
        [Key]
        public long Id { get; set; }
        public int UserId { get; set; }
        public string TelegramChatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Action { get;set; }
        public User User { get; set; }
        public DateTime? LastSendAnniversaryNotiDateTime { get; set; }
        public DateTime? LastSendExpiredNotiDateTime { get; set; }
    }
}

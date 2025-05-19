using Tasin.Website.Domains.Entitites;
using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Models.ViewModels
{
    public class HistoryLoginViewModel : BaseAuditableEntity
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string TelegramChatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Action { get; set; }
        public User User { get; set; }
        public DateTime? LastSendAnniversaryNotiDateTime { get; set; }
        public DateTime? LastSendExpiredNotiDateTime { get; set; }
    }
}

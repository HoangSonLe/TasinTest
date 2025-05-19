using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasin.Website.Domains.Entitites
{
    public class Config : BaseAuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int NumberOfDaysNoticeAnniversary { get; set; }
        public string ReminderEmailSubject { get; set; }
        public int NumberOfDaysNoticeExpiredUrn { get; set; }
        public int RemindNotification { get; set; }
        public int MonthGeneralNotification { get; set; }
        public int DayGeneralNotification { get; set; }
        public Tenant Tenant { get; set; }

    }
}

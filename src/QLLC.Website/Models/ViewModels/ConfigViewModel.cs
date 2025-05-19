using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    public class ConfigViewModel : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public int NumberOfDaysNoticeExpiredUrn { get; set; }
        public int RemindNotification { get; set; }
        public int MonthGeneralNotification { get; set; }
        public int DayGeneralNotification { get; set; }
        public int NumberOfDaysNoticeAnniversary { get; set; }
        public string ReminderEmailSubject { get; set; }
        public TenantViewModel Tenant { get; set; }
    }
}

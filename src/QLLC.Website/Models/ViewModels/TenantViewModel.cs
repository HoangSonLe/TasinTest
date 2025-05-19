using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    public class TenantViewModel : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Code { get; set; }
        public string NameNonUnicode { get; set; }
        public string AddressNonUnicode { get; set; }

    }
}

using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? NameNonUnicode { get; set; }
        public string Description { get; set; }
    }
}

using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// Base view model that extends BaseAuditableEntity with additional properties for UI display
    /// </summary>
    public abstract class BaseViewModel : BaseAuditableEntity
    {
        /// <summary>
        /// Name of the user who last updated the entity
        /// </summary>
        public string UpdatedByName { get; set; } = string.Empty;
    }
}

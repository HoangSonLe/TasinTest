using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// Base view model that extends BaseAuditableEntity with additional properties for UI display
    /// </summary>
    public abstract class BaseViewModel
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; } = 1;

        public DateTime? UpdatedDate { get; set; } = null;

        public int? UpdatedBy { get; set; } = null;

        /// <summary>
        /// Entity status
        /// </summary>
        public bool IsActive { get; set; } = true;


        /// <summary>
        /// Name of the user who last updated the entity
        /// </summary>
        public string UpdatedByName { get; set; } = string.Empty;
    }
}

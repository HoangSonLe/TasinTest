using Tasin.Website.Common.Enums;

namespace Tasin.Website.Domains.Entitites
{
    public abstract class BaseAuditableEntity
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
        /// Status string for additional status information
        /// </summary>
        public string? Status { get; set; }
    }
}

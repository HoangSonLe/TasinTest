using AutoMapper.Configuration.Annotations;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Models.ViewModels
{
    public class UserViewModel : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// Không dùng nữa
        /// </summary>
        //public int TypeAccount { get; set; }
        public List<int> RoleIdList { get; set; }
        [Ignore]
        public List<RoleViewModel> RoleViewList { get; set; }
        [Ignore]
        public List<int> EnumActionList { get; set; }
        [Ignore]
        public string RoleName { get; set; }
        public string UpdatedByName { get; set; }



    }
}

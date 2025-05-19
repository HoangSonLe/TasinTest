using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface ITenantService : IBaseService, IDisposable
    {

        Task<Acknowledgement<JsonResultPaging<List<TenantViewModel>>>> GetTenantList(UserSearchModel postData);
        Task<Acknowledgement<TenantViewModel>> GetUserById(int userId);
        Task<Acknowledgement> CreateOrUpdate(TenantViewModel postData);

        Task<Acknowledgement> DeleteTenantById(int tenantId);
    }
}

using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IConfigService : IBaseService, IDisposable
    {

        Task<Acknowledgement<JsonResultPaging<List<ConfigViewModel>>>> GetConfigList(UserSearchModel postData);
        Task<Acknowledgement> CreateOrUpdate(ConfigViewModel postData);
        Task<Acknowledgement<ConfigViewModel>> GetConfigByCurrentTenantId();


    }
}

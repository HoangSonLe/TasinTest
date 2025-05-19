using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IUrnService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnList(UrnSearchModel searchModel);
        Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnWorshipDayList(UrnSearchModel searchModel);
        //Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnConsignmentExpired(UrnSearchModel searchModel);
        Task<Acknowledgement> CreateOrUpdateUrn(UrnViewModel postData);
        Task<Acknowledgement> DeleteUrnById(long urnId);
        Task<Acknowledgement<UrnViewModel>> GetUrnById(long urnId);

    }
}

using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IHistoryLoginService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<HistoryLoginViewModel>>>> GetHistoryLoginList(HistoryLoginSearchModel searchModel);
    }
}

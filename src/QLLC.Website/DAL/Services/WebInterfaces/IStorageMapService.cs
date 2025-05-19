using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IStorageMapService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<StorageMapViewModel>>>> GetStorageMapList(StorageMapSearchModel searchModel);
        Task<Acknowledgement> CreateOrUpdateStorageMap(StorageMapViewModel postData);
        Task<Acknowledgement> DeleteStorageMapById(long storageMapId);
        Task<Acknowledgement<StorageMapViewModel>> GetStorageMapById(long storageMapId);

    }
}

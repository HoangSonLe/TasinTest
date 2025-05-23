using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IUnitService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<UnitViewModel>>>> GetUnitList(UnitSearchModel postData);
        Task<Acknowledgement<UnitViewModel>> GetUnitById(int unitId);
        Task<Acknowledgement> CreateOrUpdateUnit(UnitViewModel postData);
        Task<Acknowledgement> DeleteUnitById(int unitId);
    }
}

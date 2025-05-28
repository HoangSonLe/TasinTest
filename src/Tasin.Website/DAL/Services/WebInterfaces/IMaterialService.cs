using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IMaterialService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<MaterialViewModel>>>> GetMaterialList(MaterialSearchModel postData);
        Task<Acknowledgement<MaterialViewModel>> GetMaterialById(int materialId);
        Task<Acknowledgement> CreateOrUpdateMaterial(MaterialViewModel postData);
        Task<Acknowledgement> DeleteMaterialById(int materialId);
    }
}

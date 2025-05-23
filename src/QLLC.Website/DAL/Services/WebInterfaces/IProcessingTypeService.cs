using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IProcessingTypeService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<ProcessingTypeViewModel>>>> GetProcessingTypeList(ProcessingTypeSearchModel postData);
        Task<Acknowledgement<ProcessingTypeViewModel>> GetProcessingTypeById(int processingTypeId);
        Task<Acknowledgement> CreateOrUpdateProcessingType(ProcessingTypeViewModel postData);
        Task<Acknowledgement> DeleteProcessingTypeById(int processingTypeId);
    }
}

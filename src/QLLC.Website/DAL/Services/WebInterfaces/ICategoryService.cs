using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface ICategoryService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<CategoryViewModel>>>> GetCategoryList(CategorySearchModel postData);
        Task<Acknowledgement<CategoryViewModel>> GetCategoryById(int categoryId);
        Task<Acknowledgement> CreateOrUpdateCategory(CategoryViewModel postData);
        Task<Acknowledgement> DeleteCategoryById(int categoryId);
    }
}

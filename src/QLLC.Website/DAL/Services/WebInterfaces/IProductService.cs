using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IProductService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel postData);
        Task<Acknowledgement<ProductViewModel>> GetProductById(int productId);
        Task<Acknowledgement> CreateOrUpdateProduct(ProductViewModel postData);
        Task<Acknowledgement> DeleteProductById(int productId);
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
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
        Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel, Expression<Func<Product, object>>? selector = null, int? excludeProductId = null);
        Task<Acknowledgement<ProductViewModel>> GetProductById(int productId);
        Task<Acknowledgement> CreateOrUpdateProduct(ProductViewModel postData);
        Task<Acknowledgement> CreateProduct(ProductViewModel postData);
        Task<Acknowledgement> UpdateProduct(ProductViewModel postData);
        Task<Acknowledgement> DeleteProductById(int productId);

        /// <summary>
        /// Import products from Excel file
        /// </summary>
        /// <param name="file">Excel file</param>
        /// <returns>Import result</returns>
        Task<Acknowledgement<ProductExcelImportResult>> ImportProductsFromExcel(IFormFile file);

        /// <summary>
        /// Generate Excel template for product import
        /// </summary>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> GenerateProductExcelTemplate();
    }
}

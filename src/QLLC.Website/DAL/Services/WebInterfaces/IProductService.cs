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
